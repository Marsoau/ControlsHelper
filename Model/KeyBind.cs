using System.Drawing;

namespace ControlsHelper.Model
{
    public class KeyBind
    {
        public string Function;
        public List<int> Keys;
        public Color Paint;

        public KeyBind() {
            Keys = new List<int>();
            Paint = Color.White;
            Function = "None";
        }

        public override string ToString() {
            return $"[]({Keys.Count}) {Function}";
        }
    }
}
