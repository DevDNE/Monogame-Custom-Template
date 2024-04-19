using System.Collections.Generic;
using GraphicsManager;
using Microsoft.Xna.Framework;

namespace PhysicsManager
{
  public class CollisionManager
  {
    // Can create a list of collidables to loop through for detection and add/remove methods if needed
    // Collidables can be enemies, projectiles, obstacles, and handled differently
    // Currently only checking collisions between two sprites whenever needed

    public static bool CheckCollisions(Rectangle sprite1, Rectangle sprite2)
    {
      // Calculate the Minkowski sum of the two sprites
      int left = sprite2.Left - sprite1.Right;
      int right = sprite2.Right - sprite1.Left;
      int top = sprite2.Top - sprite1.Bottom;
      int bottom = sprite2.Bottom - sprite1.Top;

      // Check if the Minkowski sum overlaps the origin (0, 0)
      if (left <= 0 && right >= 0 && top <= 0 && bottom >= 0)
      {
        System.Console.WriteLine("Collision Detected");
        return true; // Collision detected
      }
      System.Console.WriteLine("No Collision Detected");
      return false; // No collision
    }
  }
}