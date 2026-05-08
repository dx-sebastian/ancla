using Ancla.Models;

namespace Ancla.Services;

public static class UiTextCatalog
{
    private static readonly IReadOnlyDictionary<UiTextKey, (string English, string Spanish)> Texts =
        new Dictionary<UiTextKey, (string English, string Spanish)>
        {
            [UiTextKey.WindowTitle] = ("Ancla", "Ancla"),
            [UiTextKey.Subtitle] = ("See your ports", "Mira tus puertos"),
            [UiTextKey.RefreshNow] = ("Refresh now", "Actualizar"),
            [UiTextKey.AutoRefreshOn] = ("Auto refresh every 5 seconds", "Auto actualizar cada 5 segundos"),
            [UiTextKey.AutoRefreshOff] = ("Auto refresh is off", "Auto actualizar apagado"),
            [UiTextKey.Search] = ("Search", "Buscar"),
            [UiTextKey.SearchPlaceholder] = ("Port, process, PID, framework or path", "Puerto, proceso, PID, framework o ruta"),
            [UiTextKey.QuickFilters] = ("Quick filters", "Filtros rapidos"),
            [UiTextKey.ShowProtected] = ("Show protected system ports", "Mostrar puertos protegidos"),
            [UiTextKey.StopSelectedProcess] = ("Stop selected process", "Detener proceso"),
            [UiTextKey.ReadyToScan] = ("Ready to scan", "Listo para escanear"),
            [UiTextKey.RefreshingPorts] = ("Refreshing ports...", "Actualizando puertos..."),
            [UiTextKey.RefreshFailed] = ("Refresh failed", "La actualizacion fallo"),
            [UiTextKey.UnableToReadPortsTitle] = ("Unable to read ports", "No se pudieron leer los puertos"),
            [UiTextKey.PortsLoaded] = ("{0} ports loaded", "{0} puertos cargados"),
            [UiTextKey.ShownCount] = ("{0} shown", "{0} visibles"),
            [UiTextKey.NoPortSelected] = ("No port selected", "Ningun puerto seleccionado"),
            [UiTextKey.ChooseRowHint] = ("Choose a row to inspect a process", "Elige una fila para inspeccionar un proceso"),
            [UiTextKey.PathLabel] = ("Path", "Ruta"),
            [UiTextKey.ProtectionLabel] = ("Protection", "Proteccion"),
            [UiTextKey.ProtectedProcessTitle] = ("Protected process", "Proceso protegido"),
            [UiTextKey.ConfirmStopTitle] = ("Confirm stop", "Confirmar cierre"),
            [UiTextKey.ConfirmStopBody] = ("Stop {0} (PID {1}) using port {2}?", "Detener {0} (PID {1}) usando el puerto {2}?"),
            [UiTextKey.StopFailedTitle] = ("Stop failed", "No se pudo detener"),
            [UiTextKey.StoppedProcess] = ("Stopped PID {0}", "PID {0} detenido"),
            [UiTextKey.DevColumn] = ("Dev", "Dev"),
            [UiTextKey.PortColumn] = ("Port", "Puerto"),
            [UiTextKey.ProtocolColumn] = ("Protocol", "Protocolo"),
            [UiTextKey.AddressColumn] = ("Address", "Direccion"),
            [UiTextKey.ProcessColumn] = ("Process", "Proceso"),
            [UiTextKey.PidColumn] = ("PID", "PID"),
            [UiTextKey.StateColumn] = ("State", "Estado"),
            [UiTextKey.SafetyColumn] = ("Safety", "Seguridad"),
            [UiTextKey.PathColumn] = ("Path", "Ruta")
        };

    public static string Get(UiLanguage language, UiTextKey key)
    {
        var text = Texts[key];
        return language == UiLanguage.Spanish ? text.Spanish : text.English;
    }

    public static string Format(UiLanguage language, UiTextKey key, params object[] args)
    {
        return string.Format(Get(language, key), args);
    }
}

public enum UiTextKey
{
    WindowTitle,
    Subtitle,
    RefreshNow,
    AutoRefreshOn,
    AutoRefreshOff,
    Search,
    SearchPlaceholder,
    QuickFilters,
    ShowProtected,
    StopSelectedProcess,
    ReadyToScan,
    RefreshingPorts,
    RefreshFailed,
    UnableToReadPortsTitle,
    PortsLoaded,
    ShownCount,
    NoPortSelected,
    ChooseRowHint,
    PathLabel,
    ProtectionLabel,
    ProtectedProcessTitle,
    ConfirmStopTitle,
    ConfirmStopBody,
    StopFailedTitle,
    StoppedProcess,
    DevColumn,
    PortColumn,
    ProtocolColumn,
    AddressColumn,
    ProcessColumn,
    PidColumn,
    StateColumn,
    SafetyColumn,
    PathColumn
}
