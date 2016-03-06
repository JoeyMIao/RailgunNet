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

namespace Railgun
{
  public class RailSmootherFloat : RailSmoother<float>
  {
    private static float LerpUnclampedFloat(
      float from, 
      float to, 
      float t)
    {
      return (from + ((to - from) * t));
    }

    private static ShouldSnap<float> CreateShouldSnap(
      float snappingDistance)
    {
      return delegate(float a, float b)
      {
        if ((a - b) > snappingDistance)
          return true;
        if ((b - a) > snappingDistance)
          return true;
        return false;
      };
    }

    public RailSmootherFloat(
      Accessor<float> accessor,
      float snappingDistance = float.MaxValue,
      float maxExtrapolationTime = float.MaxValue)
      : base(
        accessor,
        RailSmootherFloat.LerpUnclampedFloat,
        RailSmootherFloat.CreateShouldSnap(snappingDistance),
        maxExtrapolationTime)
    {
    }
  }
}
