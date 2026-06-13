namespace DesignCourt.Core;

public sealed record ReviewedDocument(
    string Path,
    string Content,
    IReadOnlyList<DocumentSection> Sections);

public sealed record DocumentSection(
    string SectionId,
    string Title,
    int Level,
    int LineStart,
    int LineEnd,
    string Text);
