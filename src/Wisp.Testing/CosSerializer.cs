using System.Xml;

namespace Wisp.Testing;

public sealed class CosSerializerSettings
{
    public bool ExpandStreamObjects { get; init; } = true;
}

public static class CosSerializer
{
    public static string Serialize(
        CosDocument document,
        CosSerializerSettings settings)
    {
        var output = new Utf8StringWriter();
        using (var writer = Context.Create(output, document, settings))
        {
            writer.WriteElement("Document", () =>
            {
                writer.WriteAttribute(
                    "Version",
                    document.Version.ToVersionString());

                writer.WriteComment("XRef Table");
                writer.WriteElement("XRefTable", () =>
                {
                    foreach (var xref in document.XRefTable)
                    {
                        writer.WriteElement("Entry", () =>
                        {
                            writer.WriteAttribute("Id", $"{xref.Id.Number}:{xref.Id.Generation}");

                            if (xref is CosIndirectXRef indirect)
                            {
                                if (indirect.Position != null)
                                {
                                    writer.WriteAttribute("Position", indirect.Position.ToString());
                                }
                            }
                            else if (xref is CosStreamXRef stream)
                            {
                                writer.WriteAttribute("Stream", $"{stream.StreamId.Number}:{stream.StreamId.Generation}");
                                writer.WriteAttribute("Index", stream.Index);
                            }
                            else
                            {
                                throw new InvalidOperationException("Unknown xref entry kind");
                            }
                        });
                    }
                });

                writer.WriteComment("Trailer");
                writer.WriteElement("Trailer", () =>
                {
                    document.Trailer.Accept(Visitor.Shared, writer);
                });

                writer.WriteComment("Objects");
                writer.WriteElement("Objects", () =>
                {
                    foreach (var obj in document.Objects)
                    {
                        obj.Accept(Visitor.Shared, writer);
                    }
                });
            });
        }

        return output.ToString();
    }

    private sealed class Context : XmlWriterEx
    {
        private readonly CosSerializerSettings _settings;

        public CosDocument Document { get; }
        public bool ExpandStreamObjects => _settings.ExpandStreamObjects;

        private Context(CosDocument document, CosSerializerSettings settings, XmlWriter writer)
            : base(writer)
        {
            _settings = settings;
            Document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public static Context Create(
            Utf8StringWriter writer,
            CosDocument document,
            CosSerializerSettings settings)
        {
            return new Context(
                document,
                settings,
                XmlWriter.Create(
                    writer,
                    new XmlWriterSettings
                    {
                        Indent = true,
                        OmitXmlDeclaration = true,
                    }));
        }
    }

    private sealed class Visitor : CosVisitor<Context>
    {
        public static Visitor Shared { get; } = new Visitor();

        public override void VisitObject(CosObject obj, Context context)
        {
            context.WriteComment($"Object: {obj.Id.Number}:{obj.Id.Generation}");
            context.WritePrimitive(obj, () =>
            {
                context.WriteAttribute("Id", $"{obj.Id.Number}:{obj.Id.Generation}");
                context.WriteAttribute("Cached", context.Document.Objects.Contains(obj.Id) ? "True" : "False");

                obj.Object.Accept(this, context);
            });
        }

        public override void VisitDictionary(CosDictionary obj, Context context)
        {
            context.WritePrimitive(obj, () =>
            {
                context.WriteAttribute("Count", obj.Count);

                foreach (var (key, value) in obj)
                {
                    context.WriteElement("Item", () =>
                    {
                        context.WriteAttribute("Key", key.Value);
                        value.Accept(this, context);
                    });
                }
            });
        }

        public override void VisitArray(CosArray obj, Context context)
        {
            context.WritePrimitive(obj, () =>
            {
                context.WriteAttribute("Count", obj.Count);

                foreach (var item in obj)
                {
                    context.WriteElement("Item", () =>
                    {
                        item.Accept(this, context);
                    });
                }
            });
        }

        public override void VisitString(CosString obj, Context context)
        {
            context.WriteString(obj.Value);
        }

        public override void VisitInteger(CosInteger obj, Context context)
        {
            context.WriteString(obj.Value.ToString(CultureInfo.InvariantCulture));
        }

        public override void VisitReal(CosReal obj, Context context)
        {
            context.WriteString(
                obj.Value.ToString("0.00", CultureInfo.InvariantCulture));
        }

        public override void VisitObjectReference(CosObjectReference obj, Context context)
        {
            context.WriteString($"{obj.Id.Number} {obj.Id.Generation} R");
        }

        public override void VisitBoolean(CosBoolean obj, Context context)
        {
            context.WriteString(obj.Value ? "True" : "False");
        }

        public override void VisitDate(CosDate obj, Context context)
        {
            context.WriteString(obj.Value.ToString(CultureInfo.InvariantCulture));
        }

        public override void VisitName(CosName obj, Context context)
        {
            context.WriteString("/" + obj.Value);
        }

        public override void VisitNull(CosNull obj, Context context)
        {
            context.WriteString("Null");
        }

        public override void VisitHexString(CosHexString obj, Context context)
        {
            context.WritePrimitive(obj, () =>
            {
                context.WriteString(Convert.ToHexString(obj.Value));
            });
        }

        public override void VisitObjectId(CosObjectId obj, Context context)
        {
            context.WriteString($"{obj.Number}:{obj.Generation}");
        }

        public override void VisitStream(CosStream obj, Context context)
        {
            context.WritePrimitive(obj, () =>
            {
                obj.Dictionary.Accept(this, context);
            });
        }

        public override void VisitObjectStream(CosObjectStream obj, Context context)
        {
            context.WritePrimitive(obj, () =>
            {
                context.WriteElement("Metadata", () =>
                {
                    obj.Dictionary.Accept(this, context);
                });

                context.WriteElement("Objects", () =>
                {
                    context.WriteAttribute("Count", obj.N.ToString(CultureInfo.InvariantCulture));

                    for (var index = 0; index < obj.N; index++)
                    {
                        var id = obj.GetObjectIdByIndex(index);
                        context.WriteComment($"Object: {id.Number}:{id.Generation}");

                        context.WriteElement("Object", () =>
                        {
                            context.WriteAttribute("Index", index);
                            context.WriteAttribute("Id", $"{id.Number}:{id.Generation}");
                            context.WriteAttribute("Cached", context.Document.Objects.Contains(id) ? "True" : "False");

                            if (context.ExpandStreamObjects)
                            {
                                obj.GetObjectByIndex(context.Document.Objects, index)
                                    .Object
                                    .Accept(this, context);
                            }
                            else
                            {
                                context.WriteComment("Omitted by serializer");
                            }
                        });
                    }
                });
            });
        }
    }
}