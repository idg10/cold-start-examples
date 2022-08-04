using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;

using System.IO.Pipelines;
using System.Text;
using System.Text.Json;

// The function handler that will be called for each Lambda event
//
// We're working directly with Stream for input and output here because it avoids getting
// serialization involved, and also avoids making the Lambda libraries use reflection to
// discover our handler's input and output types. (LambdaBootstrapBuilder.Create offers
// an overload specific to this Stream approach. If you use JsonElement, or rely on
// serialization, you end up hitting a generic method, and it then has to use reflection
// to work out what to do.)
//
// This table shows the memory usage for various input/output types for ReadyToRun and
// plain JIT.
//
// | Type        | R2R  | JIT  |
// |-------------|------|------|
// | Streams     | 55MB | 58MB |
// | JsonElement | 58MB | 60MB |
// | string      | 58MB | 60MB |
//
// 
byte[] Utf8QueryStringParameters = Encoding.UTF8.GetBytes("queryStringParameters");
byte[] Utf8Message = Encoding.UTF8.GetBytes("message");
Func<Stream, ILambdaContext, Task<Stream>> handler = async (Stream input, ILambdaContext context) =>
{
    bool inQueryStringParametersProperty = false;
    bool inMessageProperty = false;
    MemoryStream? outputStream = null;
    int depth = 0;

    PipeReader pr = PipeReader.Create(input);
    JsonReaderState jsonState = default;
    while (true)
    {
        ReadResult result = await pr.ReadAsync().ConfigureAwait(false);
        jsonState = ProcessBuffer(
            result,
            jsonState,
            out SequencePosition position);

        if (outputStream is not null)
        {
            outputStream.Flush();
            outputStream.Position = 0;
            return outputStream;
        }
    }

    JsonReaderState ProcessBuffer(
        in ReadResult result,
        in JsonReaderState jsonState,
        out SequencePosition position)
    {
        Utf8JsonReader r = new(result.Buffer, result.IsCompleted, jsonState);
        while (r.Read())
        {
            if (inMessageProperty)
            {
                if (r.TokenType == JsonTokenType.String)
                {
                    if (r.HasValueSequence)
                    {
                        outputStream = new((int)r.ValueSequence.Length);
                        foreach (var m in r.ValueSequence)
                        {
                            outputStream.Write(m.Span);
                        }
                    }
                    else
                    {
                        outputStream = new(r.ValueSpan.Length);
                        outputStream.Write(r.ValueSpan);
                    }

                    // We've got what we wanted!
                    break;
                }
            }

            inMessageProperty = false;

            if (r.TokenType == JsonTokenType.StartObject)
            {
                depth += 1;
            }
            else if (r.TokenType == JsonTokenType.EndObject)
            {
                depth -= 1;
            }
            else if (r.TokenType == JsonTokenType.PropertyName)
            {
                if (inQueryStringParametersProperty)
                {
                    inMessageProperty = depth == 2 &&
                        r.ValueSpan.SequenceEqual(Utf8Message);
                }
                else
                {
                    if (depth == 1)
                    {
                        inQueryStringParametersProperty = r.ValueSpan.SequenceEqual(Utf8QueryStringParameters);
                    }
                }
            }
        }

        position = r.Position;
        return r.CurrentState;
    }
};

// Build the Lambda runtime client passing in the handler to call for each
// event.
await LambdaBootstrapBuilder.Create(handler)
        .Build()
        .RunAsync();