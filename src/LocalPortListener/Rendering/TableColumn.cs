using LocalPortListener.Models;

namespace LocalPortListener.Rendering;

public sealed record TableColumn(
    string Header,
    Func<PortListener, string> ValueSelector,
    Func<PortListener, ConsoleColor> ColorSelector,
    bool RightAligned = false,
    int MaxWidth = 40);
