using System.Drawing;

namespace Ancla.Models;

public sealed record PortProcessEntry(
    string Protocol,
    string LocalAddress,
    int Port,
    string State,
    int ProcessId,
    string ProcessName,
    string ExecutablePath,
    bool IsProtected,
    string ProtectionReason,
    string DevProfileKey,
    string DevProfileLabel)
{
    public string SafetyLabel => IsProtected ? "Protected" : "Safe to stop";
    public bool IsDevelopmentPort => !string.IsNullOrWhiteSpace(DevProfileKey);
    public Image? DevProfileIcon => IsDevelopmentPort ? Services.DevelopmentPortIconFactory.Get(DevProfileKey) : null;
}
