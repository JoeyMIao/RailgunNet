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
using System.Collections.Generic;

using Reservoir;

namespace Railgun
{
  public class Entity : Poolable<Entity>
  {
    public const int INVALID_ID = -1;

    public int Id { get; internal set; }
    public State State { get; internal set; }

    public Entity() { }

    protected override void Reset()
    {
      this.Id = Entity.INVALID_ID;
      this.State = null;
    }

    /// <summary>
    /// Deep-copies this entity into the given image.
    /// </summary>
    internal void PopulateImage(Image image)
    {
      RailgunUtil.Assert(image.Id != Entity.INVALID_ID);
      RailgunUtil.Assert(image.State != null);

      image.Id = this.Id;
      image.State = this.State.Clone();
    }

    public T GetState<T>() 
      where T : State<T>
    {
      return (T)this.State;
    }
  }
}
