using System.Drawing;

namespace ControlsHelper.Model
{
    public class KeyBind
    {
        public string Action;
        public List<int> Keys;

        public KeyBind() {
            Keys = new List<int>();
            Action = "None";
        }

        public override string ToString() {
            return $"[]({Keys.Count}) {Action}";
        }
    }
}
