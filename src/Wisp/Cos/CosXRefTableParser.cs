namespace Wisp.Cos;

[PublicAPI]
public static class CosXRefTableParser
{
    public static CosXRefTable Parse(CosParser parser)
    {
        parser.Lexer.Expect(CosTokenKind.XRef);

        var table = new CosXRefTable();

        while (parser.Lexer.Reader.CanRead)
        {
            if (!parser.Lexer.Check(CosTokenKind.Integer))
            {
                break;
            }

            var startId = parser.Lexer.Expect(CosTokenKind.Integer).ParseInteger();
            var count = parser.Lexer.Expect(CosTokenKind.Integer).ParseInteger();

            foreach (var id in Enumerable.Range(startId, count))
            {
                var position = parser.Lexer.Expect(CosTokenKind.Integer).ParseInteger();
                var generation = parser.Lexer.Expect(CosTokenKind.Integer).ParseInteger();

                switch (parser.Lexer.Read().Kind)
                {
                    case CosTokenKind.XRefFree:
                        table.Add(new CosFreeXRef(new CosObjectId(id, generation)));
                        break;
                    case CosTokenKind.XRefIndirect:
                        table.Add(new CosIndirectXRef(
                            new CosObjectId(id, generation),
                            position));
                        break;
                    default:
                        throw new InvalidOperationException("Unknown xref kind encountered");
                }
            }
        }

        return table;
    }
}