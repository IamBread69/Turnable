﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TurnItUp.Locations;
using Tests.Factories;
using TurnItUp.Characters;
using System.Collections.Generic;
using TurnItUp.Components;
using System.Tuples;
using Entropy;
using Moq;
using TurnItUp.Interfaces;
using TurnItUp.Tmx;

namespace Tests.Characters
{
    [TestClass]
    public class CharacterManagerTests
    {
        private IWorld _world;
        private Board _board;
        private CharacterManager _characterManager;
        private bool _eventTriggeredFlag;

        [TestInitialize]
        public void Initialize()
        {
            _eventTriggeredFlag = false;
            _board = LocationsFactory.BuildBoard();
            _world = _board.World;
            _characterManager = (CharacterManager)_board.CharacterManager;
        }

        [TestMethod]
        public void CharacterManager_Construction_IsSuccessful()
        {
            // TODO: Check that the position of the characters is set correctly
            CharacterManager characterManager = new CharacterManager(_world, _board);

            Assert.AreEqual(characterManager.Board, _board);
            Assert.IsNotNull(characterManager.Characters);
            Assert.AreEqual(9, characterManager.Characters.Count);
            Assert.IsNotNull(characterManager.Player);

            // Are all Characters set up with a Model?
            foreach (Entity character in characterManager.Characters)
            {
                Assert.IsNotNull(character.GetComponent<Model>());
                // TODO: Test that the models are set up correctly for each character
                Assert.IsTrue(new List<String> { "Knight M", "Skeleton", "Skeleton Archer", "Pharaoh" }.Contains(character.GetComponent<Model>().Name));
            }

            // Is a TurnQueue setup with the Player taking the first turn?
            Assert.IsNotNull(characterManager.TurnQueue);
            Assert.AreEqual(9, characterManager.TurnQueue.Count);
            Assert.AreEqual(characterManager.Player, characterManager.TurnQueue[0]);

            foreach (Entity character in characterManager.Characters)
            {
                Assert.AreEqual(_board, character.GetComponent<OnBoard>().Board);
            }
        }

        [TestMethod]
        public void CharacterManager_ConstructionWhenModelPropertyIsUnsetForACharacter_IgnoresSettingUpTheModelWhenNeeded()
        {
            _board = LocationsFactory.BuildBoard("../../Fixtures/FullExampleWithUnsetModelForSomeCharacters.tmx");

            CharacterManager characterManager = new CharacterManager(_world, _board);

            Assert.AreEqual(characterManager.Board, _board);
            Assert.IsNotNull(characterManager.Characters);
            Assert.AreEqual(9, characterManager.Characters.Count);
            Assert.IsNotNull(characterManager.Player);

            // Are all Characters set up with a Model IF they had a model property in the reference tile?
            foreach (Entity character in characterManager.Characters)
            {
                if (character.GetComponent<Model>() != null)
                {
                    // TODO: Test that the models are set up correctly for each character
                    Assert.IsTrue(new List<String> { "Knight M", "Skeleton", "Skeleton Archer", "Pharaoh" }.Contains(character.GetComponent<Model>().Name));
                }
            }
        }

        [TestMethod]
        public void CharacterManager_MovingCharacterToAPosition_MovesCharacterCorrectly()
        {
            Entity character = _characterManager.Characters[0];
            Position currentPosition = character.GetComponent<Position>().DeepClone();
            Position newPosition = new Position(currentPosition.X - 1, currentPosition.Y);

            MoveResult moveResult = _characterManager.MoveCharacterTo(character, newPosition);

            Assert.AreEqual(MoveResultStatus.Success, moveResult.Status);
            Assert.AreEqual(newPosition, character.GetComponent<Position>());
            Assert.AreEqual(2, moveResult.Path.Count);
            Assert.AreEqual(currentPosition, moveResult.Path[0]);
            Assert.AreEqual(newPosition, moveResult.Path[1]);
        }

        [TestMethod]
        public void CharacterManager_MovingCharacterToAPositionOccupiedByAnObstacle_ReturnsHitObstacleMoveResultAndPositionOfObstacleToIndicateFailure()
        {
            Entity character = _characterManager.Characters[0];
            Position currentPosition = character.GetComponent<Position>().DeepClone();
            Position newPosition = new Position(currentPosition.X - 1, currentPosition.Y - 1);

            MoveResult moveResult = _characterManager.MoveCharacterTo(character, newPosition);

            // Make sure that character was NOT moved
            Assert.AreEqual(MoveResultStatus.HitObstacle, moveResult.Status);
            Assert.AreEqual(currentPosition, character.GetComponent<Position>());
            Assert.AreEqual(2, moveResult.Path.Count);
            Assert.AreEqual(currentPosition, moveResult.Path[0]);
            Assert.AreEqual(new Position(4, 0), moveResult.Path[1]);
        }

        [TestMethod]
        public void CharacterManager_MovingCharacterToAPositionOccupiedByAnotherCharacter_ReturnsHitCharacterMoveResultAndPositionOfOtherCharacterToIndicateFailure()
        {
            Entity character = _characterManager.Characters[0];
            Position currentPosition = character.GetComponent<Position>().DeepClone();
            Position newPosition = new Position(currentPosition.X + 1, currentPosition.Y);

            MoveResult moveResult = _characterManager.MoveCharacterTo(character, newPosition);

            // Make sure that character was NOT moved
            Assert.AreEqual(MoveResultStatus.HitCharacter, moveResult.Status);
            Assert.AreEqual(currentPosition, character.GetComponent<Position>());
            Assert.AreEqual(2, moveResult.Path.Count);
            Assert.AreEqual(currentPosition, moveResult.Path[0]);
            Assert.AreEqual(new Position(6, 1), moveResult.Path[1]);
        }

        [TestMethod]
        public void CharacterManager_MovingCharacterByDirection_DelegatesToMoveCharacterTo()
        {
            Entity character = _characterManager.Characters[0];
            Position currentPosition = character.GetComponent<Position>();
            Mock<CharacterManager> characterManagerMock = new Mock<CharacterManager>() { CallBase = true };
            characterManagerMock.Setup(cm => cm.MoveCharacterTo(It.IsAny<Entity>(), It.IsAny<Position>()));

            characterManagerMock.Object.MoveCharacter(character, Direction.Left);
            characterManagerMock.Verify(cm => cm.MoveCharacterTo(character, new Position(currentPosition.X - 1, currentPosition.Y)));

            characterManagerMock.Object.MoveCharacter(character, Direction.Right);
            characterManagerMock.Verify(cm => cm.MoveCharacterTo(character, new Position(currentPosition.X + 1, currentPosition.Y)));

            characterManagerMock.Object.MoveCharacter(character, Direction.Up);
            characterManagerMock.Verify(cm => cm.MoveCharacterTo(character, new Position(currentPosition.X, currentPosition.Y  - 1)));

            characterManagerMock.Object.MoveCharacter(character, Direction.Down);
            characterManagerMock.Verify(cm => cm.MoveCharacterTo(character, new Position(currentPosition.X, currentPosition.Y + 1)));
        }

        [TestMethod]
        public void CharacterManager_TryingToMovePlayer_DelegatesToMoveCharacter()
        {
            Entity player = _characterManager.Player;
            Mock<CharacterManager> characterManagerMock = new Mock<CharacterManager>() { CallBase = true };
            characterManagerMock.Setup(cm => cm.Player).Returns(player);
            characterManagerMock.Setup(cm => cm.MoveCharacter(It.IsAny<Entity>(), It.IsAny<Direction>()));

            characterManagerMock.Object.MovePlayer(Direction.Down);

            characterManagerMock.Verify(cm => cm.MoveCharacter(player, Direction.Down));
        }

        [TestMethod]
        public void CharacterManager_CanDetermineIfThereIsACharacterAtALocation()
        {
            Assert.IsTrue(_characterManager.IsCharacterAt(_characterManager.Characters[0].GetComponent<Position>().X, _characterManager.Characters[0].GetComponent<Position>().Y));
        }

        [TestMethod]
        public void CharacterManager_EndingTurn_MovesTheCurrentCharacterToTheEndOfTheTurnQueue()
        {
            Entity firstCharacter = _characterManager.TurnQueue[0];
            Entity secondCharacter = _characterManager.TurnQueue[1];

            _characterManager.EndTurn();

            Assert.AreEqual(secondCharacter, _characterManager.TurnQueue[0]);
            Assert.AreEqual(firstCharacter, _characterManager.TurnQueue[_characterManager.TurnQueue.Count - 1]);
        }

        private void SetEventTriggeredFlag(object sender, EventArgs e)
        {
            _eventTriggeredFlag = true;
        }

        [TestMethod]
        public void CharacterManager_EndingTurn_RaisesACharacterTurnEndedEvent()
        {
            // TODO: How do I check that the EntityEventArgs are correctly set?
            _characterManager.CharacterTurnEnded += this.SetEventTriggeredFlag;
            _characterManager.EndTurn();
            Assert.IsTrue(_eventTriggeredFlag);
        }

        [TestMethod]
        public void CharacterManager_MovingCharacterToAPosition_RaisesACharacterMovedEvent()
        {
            // TODO: How do I check that the EntityEventArgs are correctly set?
            _characterManager.CharacterMoved += this.SetEventTriggeredFlag;
            _characterManager.MoveCharacterTo(_characterManager.Characters[0], new Position(1, 1));
            Assert.IsTrue(_eventTriggeredFlag);
        }

        [TestMethod]
        public void CharacterManager_DestroyingACharacter_RaisesACharacterDestroyedEvent()
        {
            // TODO: How do I check that the EntityEventArgs are correctly set?
            _characterManager.CharacterDestroyed += this.SetEventTriggeredFlag;
            _characterManager.Destroy(_characterManager.Characters[0]);
            Assert.IsTrue(_eventTriggeredFlag);
        }

        [TestMethod]
        public void CharacterManager_DestroyingACharacter_RemovesItFromCharactersAndTheTurnQueue()
        {
            Entity characterToDestroy = _characterManager.Characters[0];

            _characterManager.Destroy(characterToDestroy);

            Assert.AreEqual(8, _characterManager.Characters.Count);
            Assert.IsFalse(_characterManager.Characters.Contains(characterToDestroy));
            Assert.IsFalse(_characterManager.TurnQueue.Contains(characterToDestroy));
            Assert.IsFalse(_world.EntityManager.Entities.Contains(characterToDestroy));
        }
    }
}