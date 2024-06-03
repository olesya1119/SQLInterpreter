using SQLInterpreter.Properties.FileCore;

namespace SQLInterpreter
{
    public interface IActivity
    {
        void Do(Entry entry);
    }
}