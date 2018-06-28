using Microsoft.ServiceFabric.Actors;
using System;

namespace WorkActor.Interfaces
{
    public interface IWorkActorEvents : IActorEvents
    {
        void WorkIsDone(string id);
    }
}
