using legallead.reader.service.interfaces;

namespace legallead.reader.service.services
{
    internal class WorkingServiceIndicator : IWorkingIndicator
    {
        public string SearchLocation => "";

        public void Update(bool status)
        {
            // this implementation takes no action
        }
    }
}
