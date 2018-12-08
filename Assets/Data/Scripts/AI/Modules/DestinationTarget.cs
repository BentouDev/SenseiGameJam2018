using Framework;

namespace Data.Scripts.AI.Modules
{
    public class DestinationTarget : BaseBehaviour
    {
        // dummy lol
        public enum TargetClass
        {
            Knight = 1,
            Cultist = 2,
            Demon = 4,
            Pot = 8
        }

        public TargetClass Class;
    }
}