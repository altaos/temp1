using System;
using System.Collections.Generic;
using ProtoBuf;

#if !UNITY_EDITOR
using System.Data.Linq;
#endif

namespace AliveChessLibrary.GameObjects.Abstract
{
    /// <summary>
    /// сектро на карте
    /// </summary>
    [ProtoContract]
    //[Table(Name = "dbo.map_sector")]
    public sealed class MapSector : ILocation, IEquatable<MapSector>
    {
        #region Variables

        [ProtoMember(1)]
        private int _sectorId;

        [ProtoMember(2)]
        private int _leftX;

        [ProtoMember(3)]
        private int _topY;

        [ProtoMember(4)]
        private PointTypes _mapPointType;

        [ProtoMember(5)]
        private float _wayCost;

        [ProtoMember(6)]
        private int _width;

        [ProtoMember(7)]
        private int _height;

        [ProtoMember(8)]
        private int _imageId;
#if !UNITY_EDITOR
        private EntitySet<MapPoint> _mapPoints;
#else
        private List<MapPoint> _mapPoints;
#endif
        #endregion

        #region Constructors

        public MapSector()
        {
#if !UNITY_EDITOR
            this._mapPoints = new EntitySet<MapPoint>(AttachMapPoint, DetachMapPoint);
#else
            this.MapPoints = new List<MapPoint>();
#endif
        }

        #endregion

        #region Methods

        public bool Equals(MapSector other)
        {
            return this.Id == other.Id;
        }

        /// <summary>
        /// перечислитель ячеек
        /// </summary>
        public IEnumerable<MapPoint> NextPoint()
        {
            for (int i = 0; i < MapPoints.Count; i++)
                yield return MapPoints[i];
        }

        /// <summary>
        /// добавление ячейки
        /// </summary>
        public void AddPoint(MapPoint point)
        {
            this.MapPoints.Add(point);
        }

        /// <summary>
        /// удаление ячейки
        /// </summary>
        public void RemovePoint(MapPoint point)
        {
            this.MapPoints.Remove(point);
        }

        /// <summary>
        /// триггер на добавление ячейки
        /// </summary>
        private void AttachMapPoint(MapPoint entity)
        {
            entity.MapSector = this;
        }

        /// <summary>
        /// триггер на удаление ячейки
        /// </summary>
        private void DetachMapPoint(MapPoint entity)
        {
            entity.MapSector = null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// индексатор ячеек
        /// </summary>
        public MapPoint this[int index]
        {
            get
            {
                return MapPoints[index];
            }
        }

        /// <summary>
        /// тип всех ячеек
        /// </summary>
        public PointTypes MapPointType
        {
            get { return _mapPointType; }
            set
            {
                this._mapPointType = value;
                for (int i = 0; i < MapPoints.Count; i++)
                    this.MapPoints[i].MapPointType = value;
            }
        }

        /// <summary>
        /// ширина сектора
        /// </summary>
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// высота сектора
        /// </summary>
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

#if !UNITY_EDITOR
         /// <summary>
        /// список ячеек
        /// </summary>
        public EntitySet<MapPoint> MapPoints
        {
            get { return _mapPoints; }
            set { _mapPoints.Assign(value); }
        }
#else
        public List<MapPoint> MapPoints
        {
            get { return _mapPoints; }
            set { _mapPoints = value; }
        }
#endif
        //[Column(Name = "map_sector_id", CanBeNull = false, Storage = "_sectorId",
        //  DbType = Constants.DB_INT, IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id
        {
            get { return _sectorId; }
            set
            {
                if (_sectorId != value)
                {
                    _sectorId = value;
                }
            }
        }

        //[Column(Name = "map_sector_image_id", CanBeNull = false, DbType = Constants.DB_INT,
        //  Storage = "_imageId")]
        public int ImageId
        {
            get { return _imageId; }
            set { _imageId = value; }
        }

        /// <summary>
        /// координата верхнего левого угла
        /// </summary>
        //[Column(Name = "left_x", CanBeNull = false, DbType = Constants.DB_INT, Storage = "_leftX")]
        public int X
        {
            get { return _leftX; }
            set
            {
                if (_leftX != value)
                {
                    _leftX = value;
                }
            }
        }

        /// <summary>
        /// координата верхнего левого угла
        /// </summary>
        //[Column(Name = "top_y", CanBeNull = false, DbType = Constants.DB_INT, Storage = "_topY")]
        public int Y
        {
            get { return _topY; }
            set
            {
                if (_topY != value)
                {
                    _topY = value;
                }
            }
        }

        /// <summary>
        /// стоимость прохождения всех ячеек
        /// </summary>
        //[Column(Name = "way_cost", CanBeNull = false, DbType = "float", Storage = "_wayCost")]
        public float WayCost
        {
            get { return _wayCost; }
            set
            {
                if (_wayCost != value)
                {
                    _wayCost = value;
                    for (int i = 0; i < MapPoints.Count; i++)
                        this.MapPoints[i].WayCost = value;
                }
            }
        }

        #endregion
    }
}
