using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FastWpfGrid
{
    public enum CellDecoration
    {
        None,
        StrikeOutHorizontal,
    }

    public enum TooltipVisibilityMode
    {
        Always,
        OnlyWhenTrimmed,
    }

    public enum MouseHoverBehaviours {
        HideWhenMouseOut,
        HideButtonWhenMouseOut,
        ShowAllWhenMouseOut,
    }


    public interface IFastGridCell
    {
        Color? BackgroundColor { get; }
        Color? FontColor { get; }
        bool IsItalic { get; }
        bool IsBold { get; }
        string TextData { get; }

        CellDecoration Decoration { get; }
        Color? DecorationColor { get; }

        string ToolTipText { get; }
        TooltipVisibilityMode ToolTipVisibility { get; }


        MouseHoverBehaviours MouseHoverBehaviour { get; }
        object CommandParameter { get; }

    }
}
