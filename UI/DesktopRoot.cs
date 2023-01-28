using Myra.Graphics2D.UI;

namespace BuildingGame.UI;

public sealed class DesktopRoot : Grid {
    public void BuildUI(params Widget[] widgets){
        Widgets.Clear();
        foreach (var widget in widgets){
            Widgets.Add(widget);
        }
    }
}