namespace OutOut.ViewModels.Wrappers
{
    public class SuccessHelper
    {
        public static SuccessOperationResult<T> Wrap<T>(T result)
        {
            return new SuccessOperationResult<T>(result);
        }
    }
}
