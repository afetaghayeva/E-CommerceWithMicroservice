using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Events
{
    public class IntegrationEvent
    {
        [JsonProperty]
        public Guid Id { get; private set; }
        [JsonProperty]
        public DateTime CreatdeDate { get; private set; }

        public IntegrationEvent()
        {
            Id= Guid.NewGuid();
            CreatdeDate= DateTime.Now;
        }
        [JsonConstructor]
        public IntegrationEvent(Guid id, DateTime creatdeDate)
        {
            Id = id;
            CreatdeDate = creatdeDate;
        }
    }
}
