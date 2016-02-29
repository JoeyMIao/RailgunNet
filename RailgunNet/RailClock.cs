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

using System;
using System.Collections;
using System.Collections.Generic;

using CommonTools;

namespace Railgun
{
  /// <summary>
  /// Used for keeping track of the remote peer's clock.
  /// </summary>
  internal class RailClock
  {
    public const int INVALID_TICK = -1;

    private const int DELAY_MIN = 2;
    private const int DELAY_MAX = 8;

    private int remoteRate;
    private int delayDesired;
    private int delayMin;
    private int delayMax;

    private int remoteTickLatest;
    private int remoteTickEstimated;

    private bool shouldUpdateEstimate;
    private bool shouldTick;

    public bool ShouldTick { get { return this.shouldTick; } }
    public int RemoteTickEstimated { get { return this.remoteTickEstimated; } }
    public int RemoteTickLatest { get { return this.remoteTickLatest; } }

    internal RailClock(
      int remoteSendRate = RailConfig.NETWORK_SEND_RATE,
      int delayMin = RailClock.DELAY_MIN,
      int delayMax = RailClock.DELAY_MAX)
    {
      this.remoteRate = remoteSendRate;
      this.remoteTickEstimated = 0;
      this.remoteTickLatest = RailClock.INVALID_TICK;

      this.delayMin = delayMin;
      this.delayMax = delayMax;
      this.delayDesired = ((delayMax - delayMin) / 2) + delayMin;

      this.shouldUpdateEstimate = false;
      this.shouldTick = false;
    }

    public void UpdateLatest(int latestTick)
    {
      if (latestTick > this.remoteTickLatest)
      {
        this.remoteTickLatest = latestTick;
        this.shouldUpdateEstimate = true;
        this.shouldTick = true;
      }
    }

    // See http://www.gamedev.net/topic/652186-de-jitter-buffer-on-both-the-client-and-server/
    public int Tick()
    {
      if (this.shouldTick == false)
        return 0;

      this.remoteTickEstimated++;
      if (this.shouldUpdateEstimate == false)
        return 1;

      int delta = this.remoteTickLatest - this.remoteTickEstimated;

      if (this.ShouldSnapTick(delta))
      {
        // Reset
        this.remoteTickEstimated = this.remoteTickLatest - this.delayDesired;
        return 0;
      }
      else if (delta > this.delayMax)
      {
        // Jump 1
        this.remoteTickEstimated++;
        return 2;
      }
      else if (delta < this.delayMin)
      {
        // Stall 1
        this.remoteTickEstimated--;
        return 0;
      }

      this.shouldUpdateEstimate = false;
      return 1;
    }
      
    private bool ShouldSnapTick(float delta)
    {
      if (delta < (this.delayMin - this.remoteRate))
        return true;
      if (delta > (this.delayMax + this.remoteRate))
        return true;
      return false;
    }
  }
}
