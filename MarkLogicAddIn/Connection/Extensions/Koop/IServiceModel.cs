using System.Collections.Generic;

namespace MarkLogic.Extensions.Koop
{
    public interface IServiceModel
    {
        string Id { get; }

        string Name { get; }

        string Description { get; }
    }
}
