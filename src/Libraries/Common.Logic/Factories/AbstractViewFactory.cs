using Common.Logic.Models;

namespace Common.Logic.Factories
{
    public abstract class AbstractViewFactory<T>
    {
        public abstract AbstractResponseModel CreateResponseModel(AbstractModel model);
        public abstract AbstractModel CreateModel(AbstractCreateRequestModel model);
        public abstract AbstractModel CreateModel(AbstractUpdateRequestModel model);
        public abstract AbstractModel CopyModel(AbstractModel modelTo, AbstractModel modelFrom);
    }

    public abstract class AbstractCreateRequestModel
    {

    }

    public abstract class AbstractUpdateRequestModel
    {

    }

    public abstract class AbstractResponseModel
    {

    }
}
