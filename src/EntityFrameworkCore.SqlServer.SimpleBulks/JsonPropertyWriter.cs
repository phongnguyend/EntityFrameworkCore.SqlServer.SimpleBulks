using EntityFrameworkCore.SqlServer.SimpleBulks;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using SimpleBulksJsonProperty = EntityFrameworkCore.SqlServer.SimpleBulks.JsonProperty;

public static class JsonPropertyWriter
{
    const int MaxSegments = 32; // adjust if deeper nesting is possible

    public static string Write<T>(T item, IReadOnlyDictionary<string, SimpleBulksJsonProperty> jsonProperties, bool indented = false)
    {
        var buffer = new ArrayBufferWriter<byte>();

        using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions
        {
            Indented = indented
        });

        writer.WriteStartObject();

        Span<int> prevStarts = stackalloc int[MaxSegments];
        Span<int> prevLens = stackalloc int[MaxSegments];
        int prevCount = 0;
        string prevKey = "";

        foreach (var (propertyName, jsonProperty) in jsonProperties)
        {
            if (jsonProperty.IsShadowProperty && jsonProperty.IsForeignKey)
                continue; // skip shadow FK properties

            ReadOnlySpan<char> span = jsonProperty.FullJsonPropertyName.AsSpan();

            Span<int> starts = stackalloc int[MaxSegments];
            Span<int> lens = stackalloc int[MaxSegments];

            int count = ParseSegments(span, starts, lens);

            int maxObjSeg = Math.Min(count - 1, prevCount - 1);

            int common = 0;
            while (common < maxObjSeg &&
                   SegmentEquals(prevKey.AsSpan(), prevStarts[common], prevLens[common],
                                 span, starts[common], lens[common]))
            {
                common++;
            }

            // close objects
            for (int i = prevCount - 2; i >= common; i--)
                writer.WriteEndObject();

            // open objects
            for (int i = common; i < count - 1; i++)
            {
                writer.WritePropertyName(span.Slice(starts[i], lens[i]));
                writer.WriteStartObject();
            }

            // write value
            writer.WritePropertyName(span.Slice(starts[count - 1], lens[count - 1]));
            WriteValue(writer, item, jsonProperty);

            // copy current segments to prev
            for (int i = 0; i < count; i++)
            {
                prevStarts[i] = starts[i];
                prevLens[i] = lens[i];
            }

            prevKey = jsonProperty.FullJsonPropertyName;
            prevCount = count;
        }

        for (int i = prevCount - 2; i >= 0; i--)
            writer.WriteEndObject();

        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    static int ParseSegments(ReadOnlySpan<char> key, Span<int> starts, Span<int> lens)
    {
        int seg = 0;
        int start = 0;

        for (int i = 0; i < key.Length; i++)
        {
            if (key[i] == '.')
            {
                starts[seg] = start;
                lens[seg] = i - start;
                seg++;
                start = i + 1;
            }
        }

        starts[seg] = start;
        lens[seg] = key.Length - start;
        seg++;

        return seg;
    }

    static bool SegmentEquals(
        ReadOnlySpan<char> a, int aStart, int aLen,
        ReadOnlySpan<char> b, int bStart, int bLen)
    {
        if (aLen != bLen) return false;
        return a.Slice(aStart, aLen).SequenceEqual(b.Slice(bStart, bLen));
    }

    static void WriteValue<T>(Utf8JsonWriter writer, T? item, SimpleBulksJsonProperty jsonProperty)
    {
        var value = PropertiesCache<T>.GetPropertyValue(jsonProperty.FullClrPropertyName, item, null);

        if (jsonProperty.ReaderWriter != null)
        {
            jsonProperty.ReaderWriter.ToJson(writer, value);
            return;
        }

        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;

            case string s:
                writer.WriteStringValue(s);
                break;

            case int i:
                writer.WriteNumberValue(i);
                break;

            case long l:
                writer.WriteNumberValue(l);
                break;

            case double d:
                writer.WriteNumberValue(d);
                break;

            case float f:
                writer.WriteNumberValue(f);
                break;

            case decimal m:
                writer.WriteNumberValue(m);
                break;

            case bool b:
                writer.WriteBooleanValue(b);
                break;

            case DateTime dt:
                writer.WriteStringValue(dt);
                break;

            case DateTimeOffset dto:
                writer.WriteStringValue(dto);
                break;

            case Guid g:
                writer.WriteStringValue(g);
                break;

            default:
                // fallback for complex types
                JsonSerializer.Serialize(writer, value);
                break;
        }
    }
}