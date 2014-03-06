﻿using Entropy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Tuples;
using TurnItUp.Components;
using TurnItUp.Locations;
using TurnItUp.Tmx;

namespace TurnItUp.Interfaces
{
    public interface ILevel
    {
        Map Map { get; set; }
        ICharacterManager CharacterManager { get; set; }
        IPathFinder PathFinder { get; set; }
        IWorld World { get; set; }

        void Initialize(IWorld world, string tmxPath, bool allowDiagonalMovement = false);

        // Facade methods
        bool IsObstacle(int x, int y);
        bool IsCharacterAt(int x, int y);
        MoveResult MovePlayer(Direction direction);
        MoveResult MoveCharacterTo(Entity character, Position destination);
    }
}
