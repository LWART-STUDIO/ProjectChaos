using PurrNet.Packing;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Location
{
    public struct LocationInfo:IPackedAuto
    {
        int locationID;

        public LocationInfo(int locationId)
        {
            this.locationID = locationId;
        }
    }
}
