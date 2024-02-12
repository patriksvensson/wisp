using System.Runtime.CompilerServices;

// Not my proudest moment, but we need to be able
// to test the parser and lexer until we've constructed
// an über PDF file that contains everything we're covering
// in those tests. Will do this when I have the energy...
[assembly: InternalsVisibleTo("Wisp.Tests")]
[assembly: InternalsVisibleTo("Wisp.Testing")]