﻿/*
 *  RailgunNet - A Client/Server Network State-Synchronization Layer for Games
 *  Copyright (c) 2016 - Alexander Shoulson - http://ashoulson.com
 *
 *  This software is provided 'as-is', without any express or implied
 *  warranty. In no event will the authors be held liable for any damages
 *  arising from the use of this software.
 *  Permission is granted to anyone to use this software for any purpose,
 *  including commercial applications, and to alter it and redistribute it
 *  freely, subject to the following restrictions:
 *  
 *  1. The origin of this software must not be misrepresented; you must not
 *     claim that you wrote the original software. If you use this software
 *     in a product, an acknowledgment in the product documentation would be
 *     appreciated but is not required.
 *  2. Altered source versions must be plainly marked as such, and must not be
 *     misrepresented as being the original software.
 *  3. This notice may not be removed or altered from any source distribution.
*/

#if SERVER
using System;
using System.Collections.Generic;

namespace Railgun
{
  /// <summary>
  /// Server is the core executing class on the server. It is responsible for
  /// managing connection contexts and payload I/O.
  /// </summary>
  public class RailServer : RailConnection
  {
    /// <summary>
    /// Collection of all participating clients.
    /// </summary>
    private Dictionary<IRailNetPeer, RailServerPeer> clients;

    /// <summary>
    /// Entities that have been destroyed.
    /// </summary>
    private Dictionary<EntityId, RailEntity> destroyedEntities;

    /// <summary>
    /// The server's room instance. TODO: Multiple rooms?
    /// </summary>
    private RailServerRoom serverRoom;
    private new RailServerRoom Room { get { return this.serverRoom; } }

    public RailServer() : base()
    {
      RailConnection.IsServer = true;
      this.clients = new Dictionary<IRailNetPeer, RailServerPeer>();
      this.destroyedEntities = new Dictionary<EntityId, RailEntity>();
    }

    /// <summary>
    /// Starts the server's room.
    /// </summary>
    public void StartRoom()
    {
      this.serverRoom = new RailServerRoom(this);
      this.SetRoom(this.serverRoom, Tick.START);
    }

    /// <summary>
    /// Wraps an incoming connection in a peer and stores it.
    /// </summary>
    public void AddPeer(IRailNetPeer peer, string identifier)
    {
      if (this.clients.ContainsKey(peer) == false)
      {
        RailServerPeer client = new RailServerPeer(peer, this.Interpreter);
        client.Identifier = identifier;
        client.EventReceived += base.OnEventReceived;
        client.PacketReceived += this.OnPacketReceived;
        this.clients.Add(peer, client);
        this.Room.AddController(client.Controller);
      }
    }

    /// <summary>
    /// Wraps an incoming connection in a peer and stores it.
    /// </summary>
    public void RemovePeer(IRailNetPeer peer)
    {
      if (this.clients.ContainsKey(peer))
      {
        RailServerPeer client = this.clients[peer];
        this.clients.Remove(peer);
        this.Room.RemoveController(client.Controller);

        // Revoke control of all the entities controlled by that controller
        client.Shutdown();
      }
    }

    /// <summary>
    /// Updates all entites and dispatches a snapshot if applicable. Should
    /// be called once per game simulation tick (e.g. during Unity's 
    /// FixedUpdate pass).
    /// </summary>
    public override void Update()
    {
      this.DoStart();

      foreach (RailServerPeer client in this.clients.Values)
        client.Update();

      this.Room.ServerUpdate();

      if (this.Room.Tick.IsSendTick)
      {
        this.Room.StoreStates();
        this.BroadcastPackets();
      }
    }

    internal void LogDestroyedEntity(RailEntity entity)
    {
      this.destroyedEntities.Add(entity.Id, entity);
    }

    /// <summary>
    /// Packs and sends a server-to-client packet to each peer.
    /// </summary>
    private void BroadcastPackets()
    {
      foreach (RailServerPeer clientPeer in this.clients.Values)
        clientPeer.SendPacket(
          this.Room.Tick,
          this.Room.Entities,
          this.destroyedEntities.Values);
    }

#region Packet Receive
    private void OnPacketReceived(
      RailServerPeer peer,
      IRailClientPacket packet)
    {
      foreach (RailCommandUpdate update in packet.CommandUpdates)
        this.ProcessCommandUpdate(peer, update);
    }

    private void ProcessCommandUpdate(
      RailServerPeer peer, 
      RailCommandUpdate update)
    {
      RailEntity entity;
      if (this.Room.TryGet(update.EntityId, out entity))
      {
        bool canReceive =
          (entity.Controller == peer.Controller) &&
          (entity.IsRemoving == false);

        if (canReceive)
          foreach (RailCommand command in update.Commands)
            entity.ReceiveCommand(command);
        else // Can't send commands to that entity, so dump them
          foreach (RailCommand command in update.Commands)
            RailPool.Free(command);
      }
    }
#endregion
  }
}
#endif