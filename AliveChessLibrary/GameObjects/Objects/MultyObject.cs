using System.Diagnostics;
using AliveChessLibrary.GameObjects.Abstract;
using AliveChessLibrary.GameObjects.Landscapes;
using AliveChessLibrary.Interfaces;
using AliveChessLibrary.Utility;
using ProtoBuf;

#if !UNITY_EDITOR
using System.Data.Linq;
#endif

namespace AliveChessLibrary.GameObjects.Objects
{
   // [Table(Name="dbo.multy_object")]
    public class MultyObject : IMultyPoint
    {
        private int _mapId;
        private int _mapSectorId;
        private int _multyObjectId;
        private MapSector _viewOnMap;
        private MultyObjectTypes _multyObjectType;
#if !UNITY_EDITOR
        private EntityRef<Map> _map;
#else
        private Map _map;
#endif
        public MultyObject()
        {
#if !UNITY_EDITOR
            this._map = default(EntityRef<Map>);
#else
            this.Map = null;
#endif
        }

        private void InitTest()
        {
            if (_viewOnMap == null)
                throw new AliveChessException("Object is not initialized");
        }

        public void AddView(MapSector sector)
        {
            this._viewOnMap = sector;
            foreach (MapPoint mp in sector.MapPoints)
                Map.SetObject(mp);
        }

        [ProtoBeforeSerialization]
        public void BeforeSerialization()
        {
            Trace.Assert(this.ViewOnMap != null);
        }

        [ProtoAfterDeserialization]
        public void AfterDeserialization()
        {
            this.MapSectorId = this.ViewOnMap.Id;
        }

        public int X
        {
            get
            {
                InitTest();
                return _viewOnMap.X;
            }
            set
            {
                InitTest();
                _viewOnMap.X = value;
            }
        }

        public int Y
        {
            get
            {
                InitTest();
                return _viewOnMap.Y;
            }
            set
            {
                InitTest();
                _viewOnMap.Y = value;
            }
        }

        public MapSector ViewOnMap
        {
            get
            {
                if (_viewOnMap == null && OnDeferredLoadingMapPoint != null)
                    OnDeferredLoadingMapPoint(this, this.MapSectorId);

                return this._viewOnMap;
            }
            set
            {
                if (value != null)
                {
                    if (_viewOnMap != value || _mapSectorId != value.Id)
                    {
                        this._viewOnMap = value;
                        this._mapSectorId = _viewOnMap.Id;
                    }
                }
            }
        }

        //[Column(Name = "multy_object_id", Storage = "_multyObjectId", CanBeNull = false,
        //    DbType = Constants.DB_INT, IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id
        {
            get
            {
                return this._multyObjectId;
            }
            set
            {
                if (this._multyObjectId != value)
                {
                    this._multyObjectId = value;
                }
            }
        }

#if !UNITY_EDITOR
        //[Column(Name = "map_id", Storage = "_mapId", CanBeNull = false, DbType = Constants.DB_INT)]
        public int MapId
        {
            get
            {
                return this._mapId;
            }
            set
            {
                if (this._mapId != value)
                {
                    if (this._map.HasLoadedOrAssignedValue)
                    {
                        throw new ForeignKeyReferenceAlreadyHasValueException();
                    }
                    this._mapId = value;
                }
            }
        }
#endif
        //[Column(Name = "map_sector_id", Storage = "_mapSectorId", CanBeNull = false, DbType = Constants.DB_INT)]
        public int MapSectorId
        {
            get
            {
                return this._mapSectorId;
            }
            set
            {
                if (this._mapSectorId != value)
                {
                    this._mapSectorId = value;
                }
            }
        }

        //[Column(Name = "multy_object_type", Storage = "_multyObjectType", CanBeNull = false,
        //    DbType = Constants.DB_INT)]
        public MultyObjectTypes MultyObjectType
        {
            get
            {
                return this._multyObjectType;
            }
            set
            {
                if (this._multyObjectType != value)
                {
                    this._multyObjectType = value;
                }
            }
        }

#if !UNITY_EDITOR
        //[Association(Name = "fk_multy_object_map", Storage = "_map", ThisKey = "MapId", IsForeignKey = true)]
        public Map Map
        {
            get
            {
                return this._map.Entity;
            }
            set
            {
                if (_map.Entity != value)
                {
                    if (_map.Entity != null)
                    {
                        var previousMap = _map.Entity;
                        _map.Entity = null;
                        previousMap.MultyObjects.Remove(this);
                    }
                    _map.Entity = value;
                    if (value != null)
                    {
                        _mapId = value.Id;
                    }
                    else
                    {
                        _mapId = -1;
                    }
                }
            }
        }
#else
        public Map Map
        {
            get { return _map; }
            set { _map = value; }
        }
#endif
        public event DeferredTargetedLoadingHandler<MultyObject> OnDeferredLoadingMapPoint;
    }
}
