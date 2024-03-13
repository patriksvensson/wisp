# Wisp

![Logo](resources/wisp.png)

A low-level library for interacting with PDF files.   
This is currently a work in progress.

## Example

```csharp
// Open the PDF document
var document = CosDocument.Open(
    File.OpenRead(
        "/Users/patrik/input.pdf"));

// Create a new object
var obj = new CosObject(
    document.XRefTable.GetNextId(),
    new CosDictionary()
    {
        { new CosName("/Foo"), new CosInteger(32) },
        { new CosName("/Bar"), new CosString("Patrik") },
    });

// Add the created object to the document
document.Objects.Set(obj);

// Get an object from the document and manipulate it
// In this case we know that the object 25:0 exist, but this
// will most certainly crash if you're running this as is.
var other = document.Objects.Get(number: 25, generation: 0);
((CosDictionary)other.Object)[new CosName("Baz")] = new CosString("Hello");

// Change the author of the document
document.Info.Title = new CosString("Wisp test");
document.Info.Author = new CosString("Patrik Svensson");

// Save the document
document.Save(
    File.OpenWrite("/Users/patrik/output.pdf"),
    CosCompression.Smallest);
```