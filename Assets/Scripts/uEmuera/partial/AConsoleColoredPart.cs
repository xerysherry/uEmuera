using uEmuera.Drawing;

namespace MinorShift.Emuera.GameView
{
    /// <summary>
	/// 色つき
	/// </summary>
	abstract partial class AConsoleColoredPart : AConsoleDisplayPart
    {
        public Color pColor { get { return Color; } }
        public Color pButtonColor { get { return Color; } }
    }
}
