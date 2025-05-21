using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MiningTycoon.GraphEntities;
using MiningTycoon.RuntimeData;
using MiningTycoon.Settings;
using MiningTycoon.Transport;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MiningTycoon.CoreServices
{
    public class TransportService : IInitializable
    {
        private TrainsSettings trainsSettings;
        private TrainsRuntimeData trainsData;
        private MapRuntimeData mapData;

        public event Action<int> OnResourceDelivered;
        
        public UniTask Initialize()
        {
            trainsSettings = Services.Resolve<TrainsSettings>();
            trainsData = Services.Resolve<TrainsRuntimeData>();
            mapData = Services.Resolve<MapRuntimeData>();
            mapData.OnMapLoaded += CreateTrains;
            mapData.OnChanged += MapDataChangedHandler;
            TrainsUpdate();
            return UniTask.CompletedTask;
        }

        private void MapDataChangedHandler()
        {
            FindTrainsRoutes();
        }

        private void CreateTrains()
        {
            List<Train> trains = new ();
            foreach (var trainBaseData in trainsSettings.DefaultTrains)
            {
                trains.Add(CreateTrain(trainBaseData));
            }

            var baseData = new TrainBaseData()
            {
                Capacity = trainsSettings.DefaultCapacity,
                MovementSpeed =
                    Random.Range(trainsSettings.MinMaxMovementSpeed.x, trainsSettings.MinMaxMovementSpeed.y),
                MiningDuration = Random.Range(trainsSettings.MinMaxMiningDuration.x,
                    trainsSettings.MinMaxMiningDuration.y),
            };
            trains.Add(CreateTrain(baseData));
            trainsData.SetTrains(trains);
        }
        
        private Train CreateTrain(TrainBaseData trainBaseData)
        {
            if (!mapData.TryGetRandomEdge(out Edge edge))
                return null;
            var train = new Train(trainBaseData, edge, Random.Range(0f, 1f));
            FindBestTrainRoute(train);
            train.OnUnloaded += TrainUnloadedHandler;
            return train;
        }

        private void TrainUnloadedHandler(Train train)
        {
            var station = mapData.GetStation(train.StationID);
            OnResourceDelivered?.Invoke(station.Multiplier * train.BaseData.Capacity);
        }

        private void FindTrainsRoutes()
        {
            foreach (var train in trainsData.Trains)
            {
                FindBestTrainRoute(train);
            }
        }

        private void FindBestTrainRoute(Train train)
        {
            float bestProductivity = 0;
            PathData bestPath = default;
            foreach (var station in mapData.Stations)
            {
                foreach (var mine in mapData.Mines)
                {
                    if(!mapData.TryGetPath(station.NodeId, mine.NodeId, out var path))
                        continue;
                    float productivity = CalculateProductivity(station, mine, path, train);
                    if (productivity > bestProductivity)
                    {
                        bestProductivity = productivity;
                        bestPath = path;
                    }
                }
            }
            train.SetRoute(bestPath);
        }

        private float CalculateProductivity(Station station, Mine mine, PathData pathData, Train train)
        {
            var productivity = station.Multiplier / (pathData.Distance * 2 / train.BaseData.MovementSpeed +
                                         train.BaseData.MiningDuration * mine.Multiplier);
            return productivity;
        }

        private async void TrainsUpdate()
        {
            float time = Time.time;
            while (Application.isPlaying)
            {
                float deltaTime = Time.time - time;
                time = Time.time;
                if (trainsData.Trains != null)
                {
                    foreach (var train in trainsData.Trains)
                    {
                        train.Update(deltaTime);
                    }
                }

                await UniTask.Yield();
            }
        }
        
        public void ChangeTrain(Train train, TrainBaseData baseData)
        {
            train.ChangeBaseData(baseData);
            FindBestTrainRoute(train);
        }
    }
}