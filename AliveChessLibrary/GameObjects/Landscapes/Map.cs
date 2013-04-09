using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using AliveChessLibrary.GameObjects.Abstract;
using AliveChessLibrary.GameObjects.Buildings;
using AliveChessLibrary.GameObjects.Characters;
using AliveChessLibrary.GameObjects.Objects;
using AliveChessLibrary.GameObjects.Resources;
using AliveChessLibrary.Interfaces;
using AliveChessLibrary.Utility;
using ProtoBuf;

#if !UNITY_EDITOR
using System.Data.Linq;
#endif

namespace AliveChessLibrary.GameObjects.Landscapes
{
   // [Table(Name = "dbo.map")]
    [ProtoContract]
    public class Map : ILocalizable
    {
        #region Variables

        [ProtoMember(1)]
        private int _mapId;
      
        [ProtoMember(2)]
        private int _mapSizeX;
        [ProtoMember(3)]
        private int _mapSizeY;

        private ILevel _level;
        private MapPoint[,] objects;
      
#if !UNITY_EDITOR
        private EntitySet<King> _kings;
        private EntitySet<Mine> _mines;
        private EntitySet<Castle> _castles;
        private EntitySet<Resource> _resources;
        private EntitySet<SingleObject> _singleObjects;
        private EntitySet<MultyObject> _multyObjects;
        private EntitySet<BasePoint> _basePoints;
        private EntitySet<Border> _borders;
#else
        private List<King> _kings;
        private List<Mine> _mines;
        private List<Castle> _castles;
        private List<Resource> _resources;
        private List<SingleObject> _singleObjects;
        private List<MultyObject> _multyObjects;
        private List<BasePoint> _basePoints;
        private List<Border> _borders;
#endif
        private Dictionary<int, IObserver> _observers;

        private object kingsSync = new object();
        private object minesSync = new object();
        private object resourcesSync = new object();
        private object castlesSync = new object();
        private object observerSync = new object();
        private object singleSync = new object();
        private object multySync = new object();
        private object landscapePoint = new object();
        private object borderSync = new object();
      
        #endregion

        #region Constructors

        public Map()
        {
#if !UNITY_EDITOR
            this._kings = new EntitySet<King>(AttachKing, DetachKing);
            this._mines = new EntitySet<Mine>(AttachMine, DetachMine);
            this._castles = new EntitySet<Castle>(AttachCastle, DetachCastle);
            this._resources = new EntitySet<Resource>(AttachResource, DetachResource);
            this._singleObjects = new EntitySet<SingleObject>(AttachSingleObject, DetachSingleObject);
            this._multyObjects = new EntitySet<MultyObject>(AttachMultyObject, DetachMultyObject);
            this._basePoints = new EntitySet<BasePoint>(AttachLandscapePoint, DetachLandscapePoint);
            this._borders = new EntitySet<Border>(AttachBorder, DetachBorder);
#else
            this.Kings = new List<King>();
            this.Mines = new List<Mine>();
            this.Castles = new List<Castle>();
            this.Resources = new List<Resource>();
            this.SingleObjects = new List<SingleObject>();
            this.MultyObjects = new List<MultyObject>();
            this.BasePoints = new List<BasePoint>();
            this.Borders = new List<Border>();
#endif
            this._observers = new Dictionary<int, IObserver>();
        }

        public Map(int sizeX, int sizeY)
            :this()
        {
            Initialize(sizeX, sizeY);
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            this.objects = new MapPoint[this.SizeX, this.SizeY];
        }

        public void Initialize(int sizeX, int sizeY)
        {
            this._mapSizeX = sizeX;
            this._mapSizeY = sizeY;
            this.objects = new MapPoint[this.SizeX, this.SizeY];
        }

        public void TestLocate(int x, int y)
        {
            if (!Locate(x, y)) throw new IndexOutOfRangeException();
        }

        public static MapPoint CreatePoint(Position position, ImageInfo image,
            PointTypes type, MapPoint under, float wayCost)
        {
            MapPoint point = new MapPoint();
            point.X = position.X;
            point.Y = position.Y;
            point.ImageId = image.ImageId;
            point.MapPointType = type;
            point.ObjectUnderThis = under;
            point.WayCost = wayCost;
            point.MapSector = null;
            point.Detected = false;
            return point;
        }

        public static MapPoint CreatePoint(int id, Position position, ImageInfo image,
           PointTypes type, MapPoint under, float wayCost)
        {
            MapPoint point = new MapPoint();
            point.Id = id;
            point.X = position.X;
            point.Y = position.Y;
            point.ImageId = image.ImageId;
            point.MapPointType = type;
            point.ObjectUnderThis = under;
            point.WayCost = wayCost;
            point.MapSector = null;
            point.Detected = false;
            return point;
        }

        public static MapPoint CreatePoint(int x, int y, ImageInfo image,
            PointTypes type, MapPoint under, float wayCost)
        {
            MapPoint point = new MapPoint();
            point.X = x;
            point.Y = y;
            point.ImageId = image.ImageId;
            point.MapPointType = type;
            point.ObjectUnderThis = under;
            point.WayCost = wayCost;
            point.MapSector = null;
            point.Detected = false;
            return point;
        }

        public static MapPoint CreatePoint(int id, int x, int y, ImageInfo image,
           PointTypes type, MapPoint under, float wayCost)
        {
            MapPoint point = new MapPoint();
            point.Id = id;
            point.X = x;
            point.Y = y;
            point.ImageId = image.ImageId;
            point.MapPointType = type;
            point.ObjectUnderThis = under;
            point.WayCost = wayCost;
            point.MapSector = null;
            point.Detected = false;
            return point;
        }

        public static MapPoint CreatePoint(int x, int y, int? imageId,
           PointTypes type, MapPoint under, float wayCost)
        {
            MapPoint point = new MapPoint();
            point.X = x;
            point.Y = y;
            point.ImageId = imageId;
            point.MapPointType = type;
            point.ObjectUnderThis = under;
            point.WayCost = wayCost;
            point.MapSector = null;
            point.Detected = false;
            return point;
        }

        public static MapPoint CreatePoint(int id, int x, int y, int? imageId,
          PointTypes type, MapPoint under, float wayCost)
        {
            MapPoint point = new MapPoint();
            point.Id = id;
            point.X = x;
            point.Y = y;
            point.ImageId = imageId;
            point.MapPointType = type;
            point.ObjectUnderThis = under;
            point.WayCost = wayCost;
            point.MapSector = null;
            point.Detected = false;
            return point;
        }

        public static MapPoint CreatePoint(Position position, MapSector sector,
            PointTypes type, MapPoint under, float wayCost)
        {
            MapPoint point = new MapPoint();
            point.X = position.X;
            point.Y = position.Y;
            point.MapPointType = type;
            point.ObjectUnderThis = under;
            point.WayCost = wayCost;
            point.MapSector = sector;
            point.Detected = false;
            point.ImageId = null;
            return point;
        }

        public static MapPoint CreatePoint(int id, Position position, MapSector sector,
           PointTypes type, MapPoint under, float wayCost)
        {
            MapPoint point = new MapPoint();
            point.Id = id;
            point.X = position.X;
            point.Y = position.Y;
            point.MapPointType = type;
            point.ObjectUnderThis = under;
            point.WayCost = wayCost;
            point.MapSector = sector;
            point.Detected = false;
            point.ImageId = null;
            return point;
        }

        public static MapPoint CreatePoint(int x, int y, MapSector sector,
           PointTypes type, MapPoint under, float wayCost)
        {
            MapPoint point = new MapPoint();
            point.X = x;
            point.Y = y;
            point.MapPointType = type;
            point.ObjectUnderThis = under;
            point.WayCost = wayCost;
            point.MapSector = sector;
            point.Detected = false;
            point.ImageId = null;
            return point;
        }

        public static MapPoint CreatePoint(int id, int x, int y, MapSector sector,
          PointTypes type, MapPoint under, float wayCost)
        {
            MapPoint point = new MapPoint();
            point.Id = id;
            point.X = x;
            point.Y = y;
            point.MapPointType = type;
            point.ObjectUnderThis = under;
            point.WayCost = wayCost;
            point.MapSector = sector;
            point.Detected = false;
            point.ImageId = null;
            return point;
        }

        public static MapSector CreateSector(Position leftTopCorner, ImageInfo image,
            PointTypes type, float wayCost)
        {
            MapSector sector = new MapSector();
            sector.X = leftTopCorner.X;
            sector.Y = leftTopCorner.Y;
            sector.Width = image.Width.Value;
            sector.Height = image.Height.Value;
            sector.ImageId = image.ImageId.Value;
            sector.MapPointType = type;
            sector.WayCost = wayCost;
            return sector;
        }

        public static MapSector CreateSector(int id, Position leftTopCorner, ImageInfo image,
           PointTypes type, float wayCost)
        {
            MapSector sector = new MapSector();
            sector.Id = id;
            sector.X = leftTopCorner.X;
            sector.Y = leftTopCorner.Y;
            sector.Width = image.Width.Value;
            sector.Height = image.Height.Value;
            sector.ImageId = image.ImageId.Value;
            sector.MapPointType = type;
            sector.WayCost = wayCost;
            return sector;
        }

        public void InitializeSector(MapSector sector)
        {
            for (int i = sector.X; i < sector.X + sector.Width; i++)
            {
                for (int j = sector.Y; j < sector.Y + sector.Height; j++)
                {
                    MapPoint point = CreatePoint(sector.Id, i, j, sector, sector.MapPointType,
                                                null, sector.WayCost);
                    sector.AddPoint(point);
                    this.SetObject(point);
                }
            }
        }
      
        #region Searchers

        public King SearchKingById(int id)
        {
            lock (kingsSync)
                return Kings.FindElement(x => x.Id == id);
        }

        public Mine SearchMineById(int id)
        {
            lock (minesSync)
                return Mines.FindElement(x => x.Id == id);
        }

        public Castle SearchCastleById(int id)
        {
            lock (castlesSync)
                return Castles.FindElement(x => x.Id == id);
        }

        public Resource SearchResourceById(int id)
        {
            lock (resourcesSync)
                return Resources.FindElement(x => x.Id == id);
        }

        public King SearchKingByPointId(int id)
        {
            lock (kingsSync)
                return Kings.FindElement(x => x.MapPointId == id);
        }

        public Mine SearchMineByPointId(int id)
        {
            lock (minesSync)
                return Mines.FindElement(x => x.MapSectorId == id);
        }

        public Castle SearchCastleByPointId(int id)
        {
            lock (castlesSync)
                return Castles.FindElement(x => x.MapSectorId == id);
        }

        public Resource SearchResourceByPointId(int id)
        {
            lock (resourcesSync)
                return Resources.FindElement(x => x.MapPointId == id);
        }

        public IObserver SearchObserverById(int observerId, PointTypes observerType)
        {
            Debug.Assert(observerType != PointTypes.None);
            if (observerType == PointTypes.King)
                return Kings.FindElement(x => x.Id == observerId);
            else
            {
                if (observerType == PointTypes.Castle)
                    return Castles.FindElement(x => x.Id == observerId);
                else
                {
                    if (observerType == PointTypes.Mine)
                        return Mines.FindElement(x => x.Id == observerId);
                    else throw new AliveChessException("Observer not found");
                }
            }
        }

        #endregion

        #region Add And Remove

        public void AddMine(Mine mine)
        {
            lock (minesSync)
                Mines.Add(mine);

            lock (minesSync)
                _observers.Add(mine.Id, mine);
        }

        public void AddCastle(Castle castle)
        {
            lock (castlesSync)
                Castles.Add(castle);

            lock (observerSync)
                _observers.Add(castle.Id, castle);
        }

        public void AddKing(King king)
        {
            lock (kingsSync)
                Kings.Add(king);

            lock (observerSync)
                _observers.Add(king.Id, king);
        }

        public void AddResource(Resource resource)
        {
            lock (resourcesSync)
                Resources.Add(resource);
        }

        public void AddSingleObject(SingleObject singleObject)
        {
            lock (singleSync)
                SingleObjects.Add(singleObject);
        }

        public void AddMultyObject(MultyObject multyObject)
        {
            lock (multySync)
                MultyObjects.Add(multyObject);
        }

        public void AddLandscapePoint(BasePoint point)
        {
            lock (landscapePoint)
                BasePoints.Add(point);
        }

        public void AddBorder(Border point)
        {
            lock (borderSync)
                Borders.Add(point);
        }

        public void RemoveKing(King king)
        {
            lock (kingsSync)
                Kings.Remove(king);

            lock (observerSync)
                _observers.Remove(king.Id);

            Debug.Assert(king.ViewOnMap != null);

            if (king.ViewOnMap.ObjectUnderThis != null)
                SetObject(king.ViewOnMap.ObjectUnderThis);
        }

        public void RemoveCastle(Castle castle)
        {
            lock (castlesSync)
                Castles.Remove(castle);

            lock (observerSync)
                _observers.Remove(castle.Id);

            Debug.Assert(castle.ViewOnMap != null);

            foreach (MapPoint mp in castle.ViewOnMap.NextPoint())
                if (mp.ObjectUnderThis != null)
                    SetObject(mp.ObjectUnderThis);
        }

        public void RemoveMine(Mine mine)
        {
            lock (minesSync)
                Mines.Remove(mine);

            lock (observerSync)
                _observers.Remove(mine.Id);

            Debug.Assert(mine.ViewOnMap != null);

            foreach (MapPoint mp in mine.ViewOnMap.NextPoint())
                if (mp.ObjectUnderThis != null)
                    SetObject(mp.ObjectUnderThis);
        }

        public void RemoveResource(Resource resource)
        {
            lock (resourcesSync)
                Resources.Remove(resource);

            Debug.Assert(resource.ViewOnMap != null);

            //foreach (MapPoint mp in resource.ViewOnMap.MapPoints)
            if (resource.ViewOnMap.ObjectUnderThis != null)
                SetObject(resource.ViewOnMap.ObjectUnderThis);
        }

        public void RemoveSingleObject(SingleObject single)
        {
            lock (singleSync)
                SingleObjects.Remove(single);

            Debug.Assert(single.ViewOnMap != null);

            if (single.ViewOnMap.ObjectUnderThis != null)
                SetObject(single.ViewOnMap.ObjectUnderThis);
        }

        public void RemoveMultyObject(MultyObject multy)
        {
            lock (multySync)
                MultyObjects.Remove(multy);

            Debug.Assert(multy.ViewOnMap != null);

            foreach (MapPoint mp in multy.ViewOnMap.NextPoint())
                if (mp.ObjectUnderThis != null)
                    SetObject(mp.ObjectUnderThis);
        }

        public void RemoveLandscapePoint(BasePoint point)
        {
            lock (landscapePoint)
                BasePoints.Remove(point);
        }

        public void RemoveBorder(Border point)
        {
            lock (borderSync)
                Borders.Remove(point);
        }

        #endregion

        #region Check Existance

        public bool ContainsMine(long id)
        {
            lock (minesSync)
            {
                foreach (Mine m in Mines)
                    if (m.Equals(id)) return true;
            }
            return false;
        }

        public bool ContainsKing(long id)
        {
            lock (kingsSync)
            {
                foreach (King k in Kings)
                    if (k.Equals(id)) return true;
            }
            return false;
        }

        public bool ContainsCastle(long id)
        {
            lock (castlesSync)
            {
                foreach (Castle c in Castles)
                    if (c.Equals(id)) return true;
            }
            return false;
        }

        public bool ContainsResource(long id)
        {
            lock (resourcesSync)
            {
                foreach (Resource r in Resources)
                    if (r.Equals(id)) return true;
            }
            return false;
        }

        #endregion

        #region Enumerators

        public IEnumerable NextKing()
        {
            lock (kingsSync)
            {
                for (int i = 0; i < Kings.Count; i++)
                    yield return Kings[i];
            }
        }

        public IEnumerable NextCastle()
        {
            lock (castlesSync)
            {
                for (int i = 0; i < Castles.Count; i++)
                    yield return Castles[i];
            }
        }

        public IEnumerable NextMine()
        {
            lock (minesSync)
            {
                for (int i = 0; i < Mines.Count; i++)
                    yield return Mines[i];
            }
        }

        public IEnumerable NextBorder()
        {
            lock (borderSync)
            {
                for (int i = 0; i < Borders.Count; i++)
                    yield return Borders[i];
            }
        }

        #endregion

        /// <summary>
        /// поиск пустого замка
        /// </summary>
        /// <returns></returns>
        public Castle SearchFreeCastle()
        {
            return Castles.FindElement(x => x.IsFree);
        }

        /// <summary>
        /// добавляем объект на карту
        /// </summary>
        /// <param name="x">координата x</param>
        /// <param name="y">координата y</param>
        public void SetObject(MapPoint obj)
        {
            objects[obj.X, obj.Y] = obj;
        }

        /// <summary>
        /// получаем стоимость прохождения через ячейку
        /// </summary>
        /// <param name="x">координата x</param>
        /// <param name="y">координата y</param>
        /// <returns></returns>
        public float GetWayCost(int x, int y)
        {
            return objects[x, y].WayCost;
        }

        /// <summary>
        /// проверка принадлежности ячейки карте
        /// </summary>
        /// <param name="x">координата x</param>
        /// <param name="y">координата y</param>
        /// <returns></returns>
        public bool Locate(int x, int y)
        {
            return x >= 1 && x < this._mapSizeX - 1 && y >= 1 && y < this._mapSizeY - 1;
        }

        public bool CheckPoint(int x, int y, int costLimit)
        {
            return Locate(x, y) && objects[x, y].WayCost <= costLimit;
        }

        #region Attach handlers

        private void AttachMine(Mine entity)
        {
            entity.Map = this;
        }

        private void DetachMine(Mine entity)
        {
            entity.Map = null;
        }

        private void AttachCastle(Castle entity)
        {
            entity.Map = this;
        }

        private void DetachCastle(Castle entity)
        {
            entity.Map = null;
        }

        private void AttachKing(King entity)
        {
            entity.Map = this;
        }

        private void DetachKing(King entity)
        {
            entity.Map = null;
        }

        private void AttachResource(Resource entity)
        {
            entity.Map = this;
        }

        private void DetachResource(Resource entity)
        {
            entity.Map = null;
        }

        private void AttachSingleObject(SingleObject entity)
        {
            entity.Map = this;
        }

        private void DetachSingleObject(SingleObject entity)
        {
            entity.Map = null;
        }

        private void AttachMultyObject(MultyObject entity)
        {
            entity.Map = this;
        }

        private void DetachMultyObject(MultyObject entity)
        {
            entity.Map = null;
        }

        private void AttachLandscapePoint(BasePoint entity)
        {
            entity.Map = this;
        }

        private void DetachLandscapePoint(BasePoint entity)
        {
            entity.Map = null;
        }

        private void AttachBorder(Border entity)
        {
            entity.Map = this;
        }

        private void DetachBorder(Border entity)
        {
            entity.Map = null;
        }

        #endregion

        #endregion

        #region Properties

        public ILevel Level
        {
            get { return _level; }
            set
            {
                if (_level != value)
                {
                    _level = value;
                    _level.Map = this;
                }
            }
        }

        public MapPoint this[int x, int y]
        {
            get
            {
                return objects[x, y];
            }
            set
            {
                objects[x, y] = value;
            }
        }

        public MapPoint this[Position position]
        {
            get
            {
                return objects[position.X, position.Y];
            }
            set
            {
                objects[position.X, position.Y] = value;
            }
        }

        //[Column(Name = "map_id", Storage = "_mapId", CanBeNull = false, DbType = Constants.DB_INT,
        //    IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id
        {
            get
            {
                return this._mapId;
            }
            set
            {
                if (this._mapId != value)
                {
                    this._mapId = value;
                }
            }
        }

        //[Column(Name = "map_sizeX", Storage = "_mapSizeX", CanBeNull = false, DbType = Constants.DB_INT)]
        public int SizeX
        {
            get
            {
                return this._mapSizeX;
            }
            set
            {
                if (this._mapSizeX != value)
                {
                    this._mapSizeX = value;
                }
            }
        }

        //[Column(Name = "map_sizeY", Storage = "_mapSizeY", CanBeNull = false, DbType = Constants.DB_INT)]
        public int SizeY
        {
            get
            {
                return this._mapSizeY;
            }
            set
            {
                if (this._mapSizeY != value)
                {
                    this._mapSizeY = value;
                }
            }
        }

#if !UNITY_EDITOR
        //[Association(Name = "fk_castle_map", Storage = "_castles", OtherKey = "MapId")]
        public EntitySet<Castle> Castles
        {
            get
            {
                return this._castles;
            }
            set
            {
                this._castles.Assign(value);
            }
        }

        //[Association(Name = "fk_king_map", Storage = "_kings", OtherKey = "MapId")]
        public EntitySet<King> Kings
        {
            get
            {
                return this._kings;
            }
            set
            {
                this._kings.Assign(value);
            }
        }

        //[Association(Name = "fk_mine_map", Storage = "_mines", OtherKey = "MapId")]
        public EntitySet<Mine> Mines
        {
            get
            {
                return this._mines;
            }
            set
            {
                this._mines.Assign(value);
            }
        }

        //[Association(Name = "fk_resource_map", Storage = "_resources", OtherKey = "MapId")]
        public EntitySet<Resource> Resources
        {
            get
            {
                return this._resources;
            }
            set
            {
                this._resources.Assign(value);
            }
        }

        //[Association(Name = "fk_single_object_map", Storage = "_singleObjects", OtherKey = "MapId")]
        public EntitySet<SingleObject> SingleObjects
        {
            get
            {
                return this._singleObjects;
            }
            set
            {
                this._singleObjects.Assign(value);
            }
        }

        //[Association(Name = "fk_multy_object_map", Storage = "_multyObjects", OtherKey = "MapId")]
        public EntitySet<MultyObject> MultyObjects
        {
            get
            {
                return this._multyObjects;
            }
            set
            {
                this._multyObjects.Assign(value);
            }
        }

        //[Association(Name = "fk_landscape_point_map", Storage = "_basePoints", OtherKey = "MapId")]
        public EntitySet<BasePoint> BasePoints
        {
            get
            {
                return this._basePoints;
            }
            set
            {
                this._basePoints.Assign(value);
            }
        }

        //[Association(Name = "fk_border_point_map", Storage = "_borders", OtherKey = "MapId")]
        public EntitySet<Border> Borders
        {
            get
            {
                return this._borders;
            }
            set
            {
                this._borders.Assign(value);
            }
        }
#else
        public List<King> Kings
        {
            get { return _kings; }
            set { _kings = value; }
        }

        public List<Mine> Mines
        {
            get { return _mines; }
            set { _mines = value; }
        }

        public List<Castle> Castles
        {
            get { return _castles; }
            set { _castles = value; }
        }

        public List<Resource> Resources
        {
            get { return _resources; }
            set { _resources = value; }
        }

        public List<SingleObject> SingleObjects
        {
            get { return _singleObjects; }
            set { _singleObjects = value; }
        }

        public List<MultyObject> MultyObjects
        {
            get { return _multyObjects; }
            set { _multyObjects = value; }
        }

        public List<BasePoint> BasePoints
        {
            get { return _basePoints; }
            set { _basePoints = value; }
        }

        public List<Border> Borders
        {
            get { return _borders; }
            set { _borders = value; }
        }
#endif
        #endregion
    }
}