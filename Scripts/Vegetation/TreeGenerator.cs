// Copyright (C) by Upvoid Studios
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>

using System;
using Engine;
using Engine.Resources;
using Engine.Universe;
using Engine.Rendering;
using Engine.Physics;

namespace UpvoidMiner
{
    /// <summary>
    /// A class that can spawn different types of trees.
    /// </summary>
    public class TreeGenerator
    {

        public static Tree Cactus(Random random, mat4 transform1, mat4 transform2, World world)
        {
            // Compute random cactus type \in 0..5
            int type = (int)(random.NextDouble()*6.0);

            // Circumvent the unlikely case of NextDouble() returning 1.0
            if(type > 5) type = 5;

            // 0..5 -> 1..6
            ++type;

            string meshString = "Vegetation/Cactus/Cactus" + type.ToString();

            MeshRenderJob cactus = new MeshRenderJob(
                Renderer.Opaque.Mesh,
                Resources.UseMaterial("Cactus", UpvoidMiner.ModDomain),
                Resources.UseMesh(meshString, UpvoidMiner.ModDomain),
                transform2);

            MeshRenderJob cactusShadow = new MeshRenderJob(
                Renderer.Shadow.Mesh,
                Resources.UseMaterial("Cactus.Shadow", UpvoidMiner.ModDomain),
                Resources.UseMesh(meshString, UpvoidMiner.ModDomain),
                transform2);

            // Add some color variance to cacti
            vec4 colorModulation = new vec4(0.7f + (float)random.NextDouble() * 0.6f, 0.9f + (float)random.NextDouble() * 0.2f, 1, 1);
            cactus.SetColor("uColorModulation", colorModulation);

            // Create new Tree of type Cactus with 0 wood to gather.
            Tree t = new Tree(0, Tree.TreeType.Cactus);
            Tree.Log l = new Tree.Log();
            RigidBody b = new RigidBody(0f, transform1 * mat4.Translate(new vec3(0,5,0)), new CylinderShape(.5f, 10));
            world.Physics.AddRigidBody(b);
            l.PhysicsComps.Add(new PhysicsComponent(b, mat4.Translate(new vec3(0,-5,0))));

            t.RjLeaves0.Add(new RenderComponent(cactus, transform2));
            t.RjLeaves0.Add(new RenderComponent(cactusShadow, transform2));

            t.Logs.Add(l);

            return t;
        }

        public static Tree OldTree(Random random, mat4 transform1, mat4 transform2, World world)
        {
            double randy = random.NextDouble();
            string trunkMesh, leavesMesh;
            if(randy < 0.34)
            {
                trunkMesh = "Vegetation/Tree01/Trunk";
                leavesMesh = "Vegetation/Tree01/Leaves_medium";
            }
            else if(randy < 0.67)
            {
                trunkMesh = "Vegetation/Tree02/Trunk";
                leavesMesh = "Vegetation/Tree02/Leaves_low";
            }
            else
            {
                trunkMesh = "Vegetation/Tree03/Trunk";
                leavesMesh = "Vegetation/Tree03/Leaves_medium";
            }

            MeshRenderJob leavesOpaque = new MeshRenderJob(
                Renderer.Opaque.Mesh,
                Resources.UseMaterial("TreeLeaves01", UpvoidMiner.ModDomain),
                Resources.UseMesh(leavesMesh, UpvoidMiner.ModDomain),
                transform2);

            /*
            MeshRenderJob leavesZPre = new MeshRenderJob(
                Renderer.zPre.Mesh,
                Resources.UseMaterial("TreeLeaves01.zPre", UpvoidMiner.ModDomain),
                Resources.UseMesh(leavesMesh, UpvoidMiner.ModDomain),
                transform2);
             */

            MeshRenderJob leavesShadow = new MeshRenderJob(
                Renderer.Shadow.Mesh,
                Resources.UseMaterial("TreeLeaves01.Shadow", UpvoidMiner.ModDomain),
                Resources.UseMesh(leavesMesh, UpvoidMiner.ModDomain),
                transform2);

            MeshRenderJob trunkOpaque = new MeshRenderJob(
                Renderer.Opaque.Mesh,
                Resources.UseMaterial("TreeTrunk", UpvoidMiner.ModDomain),
                Resources.UseMesh(trunkMesh, UpvoidMiner.ModDomain),
                transform2);

            MeshRenderJob trunkShadow = new MeshRenderJob(
                Renderer.Shadow.Mesh,
                Resources.UseMaterial("::Shadow", UpvoidMiner.ModDomain),
                Resources.UseMesh(trunkMesh, UpvoidMiner.ModDomain),
                transform2);


            // Add some color variance to trees
            vec4 colorModulation = new vec4(0.7f + (float)random.NextDouble() * 0.5f, 0.7f + (float)random.NextDouble() * 0.5f, 1, 1);
            leavesOpaque.SetColor("uColorModulation", colorModulation);

            float amountOfWood = ((float)randy * 0.7f + 0.5f) * transform2.col1.Length;
            // Amount of wood depends on tree type (thicker/thinner trunk) and tree height scale factor.
            Tree t = new Tree(amountOfWood, Tree.TreeType.Birch);
            Tree.Log l = new Tree.Log();
            RigidBody b = new RigidBody(0f, transform1 * mat4.Translate(new vec3(0,5,0)), new CylinderShape(.5f, 10));
            world.Physics.AddRigidBody(b);
            l.PhysicsComps.Add(new PhysicsComponent(b, mat4.Translate(new vec3(0,-5,0))));

            RenderComponent rc1 = new RenderComponent(leavesOpaque, transform2);
            RenderComponent rc2 = new RenderComponent(leavesShadow, transform2);
            RenderComponent rc3 = new RenderComponent(trunkOpaque, transform2);
            RenderComponent rc4 = new RenderComponent(trunkShadow, transform2);

            int maxTreeDistanceSetting = Settings.settings.MaxTreeDistance;
            float fadeOutMin = Math.Max(5, maxTreeDistanceSetting - 5);     // >= 5
            float fadeOutMax = Math.Max(10, maxTreeDistanceSetting + 5);    // >= 10
            float fadeTime = 1.0f; // 1 second

            rc1.ConfigureLod(0, 0, fadeOutMin, fadeOutMax, fadeTime);
            rc2.ConfigureLod(0, 0, fadeOutMin, fadeOutMax, fadeTime);
            rc3.ConfigureLod(0, 0, fadeOutMin, fadeOutMax, fadeTime);
            rc4.ConfigureLod(0, 0, fadeOutMin, fadeOutMax, fadeTime);

            t.RjLeaves0.Add(rc1);
            //t.RjLeaves0.Add(new RenderComponent(leavesZPre, transform2));
            t.RjLeaves0.Add(rc2);
            t.RjTrunk.Add(rc3);
            t.RjTrunk.Add(rc4);

            t.Logs.Add(l);

            return t;
        }

        /// <summary>
        /// Creates a log.
        /// </summary>
        private static Tree.Log CreateLog(Tree t,
                                          vec3 start, vec3 dir, vec3 front, float height, float radius,
                                          MaterialResource material, string meshName)
        {
            Tree.Log log = new Tree.Log();

            vec3 left = vec3.cross(dir, front);
            mat4 transform = new mat4(left, dir, front, start) * mat4.Scale(new vec3(radius, height, radius));
            var mesh = Resources.UseMesh(meshName, UpvoidMiner.ModDomain);
            
            log.RenderComps.Add(new RenderComponent( new MeshRenderJob(Renderer.Opaque.Mesh, material, mesh, mat4.Identity), transform));
            log.RenderComps.Add(new RenderComponent( new MeshRenderJob(Renderer.Shadow.Mesh, Resources.UseMaterial("::Shadow", null), mesh, mat4.Identity), transform));
            log.RenderComps.Add(new RenderComponent( new MeshRenderJob(Renderer.zPre.Mesh, Resources.UseMaterial("::ZPre", null), mesh, mat4.Identity), transform));

            return log;
        }

        /// <summary>
        /// Creates a birch tree
        /// </summary>
        /// <param name="height">Height in m.</param>
        /// <param name="width">radius in m.</param>
        public static Tree Birch(float height, float radius, Random random)
        {
            Tree t = new Tree();
            
            SeedPointMeshRenderJob foliageJob = new SeedPointMeshRenderJob(
                Renderer.Opaque.Mesh,
                Resources.UseMaterial("SimpleBirchLeaves", UpvoidMiner.ModDomain),
                Resources.UseMesh("Vegetation/Leaves", UpvoidMiner.ModDomain),
                mat4.Identity);
            SeedPointMeshRenderJob foliageJob2 = new SeedPointMeshRenderJob(
                Renderer.Transparent.Mesh,
                Resources.UseMaterial("SimpleBirchLeaves.Transparent", UpvoidMiner.ModDomain),
                Resources.UseMesh("Vegetation/Leaves", UpvoidMiner.ModDomain),
                mat4.Identity);
            SeedPointMeshRenderJob foliageJob3 = new SeedPointMeshRenderJob(
                Renderer.zPre.Mesh,
                Resources.UseMaterial("BirchLeaves.zPre", UpvoidMiner.ModDomain),
                Resources.UseMesh("Vegetation/Leaves", UpvoidMiner.ModDomain),
                mat4.Identity);

            float hsum = 0;
            float unitHeight = radius * 2 * (float)Math.PI * 2 * .6f;
            MaterialResource mat = Resources.UseMaterial("Vegetation/BirchLog", UpvoidMiner.ModDomain);
            while (hsum < height)
            {
                float h = unitHeight * (.8f + (float)random.NextDouble() * .4f);
                t.Logs.Add(CreateLog(t, new vec3(0, hsum, 0), vec3.UnitY, vec3.UnitZ, h, radius, mat, "Vegetation/Trunk-1.0"));

                int leaves = (int)(0 + (hsum / height) * (8 + random.Next(0, 3)));
                for (int i = 0; i < leaves * 4; ++i)
                {
                    vec3 rad = new vec3((float)random.NextDouble() - .5f, 0, (float)random.NextDouble() - .5f).Normalized;

                    vec3 pos = new vec3(0, hsum + (float)random.NextDouble() * h, 0) + rad * radius * .9f;
                    vec3 normal = (rad + new vec3(0,.3f - (float)random.NextDouble() * .6f,0)).Normalized * (1 + (float)random.NextDouble() * (.2f + hsum/height * .3f));
                    vec3 tangent = vec3.cross(normal, new vec3((float)random.NextDouble() - .5f, (float)random.NextDouble() - .5f, (float)random.NextDouble() - .5f)).Normalized * (1 + (float)random.NextDouble() * (.2f + hsum/height * .3f));
                    vec3 color = new vec3(.9f + (float)random.NextDouble() * .4f, 1, 1);
                    
                    foliageJob.AddSeed(pos, normal, tangent, color);
                    foliageJob2.AddSeed(pos, normal, tangent, color);
                    foliageJob3.AddSeed(pos, normal, tangent, color);
                }
                
                hsum += h; 
            }

            int branches = random.Next(3, 5);
            for (int i = 0; i < branches; ++i)
            {
                float h = (float)((1 - random.NextDouble() * random.NextDouble()) * hsum) * .8f + .1f;
                vec3 dir = new vec3((float)random.NextDouble() * 2  - 1, .3f + (float)random.NextDouble() * .6f, (float)random.NextDouble() * 2 - 1).Normalized;
                vec3 front = vec3.cross(dir, vec3.UnitY).Normalized;
                vec3 left = vec3.cross(front, dir);
                float r = radius * (0.2f + (float)random.NextDouble() * .4f);
                vec3 basePos = new vec3(0, h, 0);
                float branchLength = unitHeight * (.8f + (float)random.NextDouble() * .4f + .1f) * .7f;
                t.Logs.Add(CreateLog(t, basePos, dir, front, branchLength, r, mat, "Vegetation/Trunk-0.8")); 
                
                int leaves = (int)(4 + random.Next(0, 3));
                for (int j = 0; j < leaves * 4; ++j)
                {
                    vec3 rad = (((float)random.NextDouble() - .5f) * left + ((float)random.NextDouble() - .5f) * front).Normalized;

                    vec3 pos = basePos + dir * branchLength * (float)random.NextDouble() + rad * r * .9f;
                    vec3 normal = (rad + new vec3(0,.3f - (float)random.NextDouble() * .6f,0)).Normalized * (1 + (float)random.NextDouble() * (.2f + hsum/height * .3f));
                    vec3 tangent = vec3.cross(normal, new vec3((float)random.NextDouble() - .5f, (float)random.NextDouble() - .5f, (float)random.NextDouble() - .5f)).Normalized * (1 + (float)random.NextDouble() * (.2f + hsum/height * .3f));
                    vec3 color = new vec3(.9f + (float)random.NextDouble() * .4f, 1, 1);

                    foliageJob.AddSeed(pos, normal, tangent, color);
                    foliageJob2.AddSeed(pos, normal, tangent, color);
                    foliageJob3.AddSeed(pos, normal, tangent, color);
                }
            }
            
            foliageJob.FinalizeSeeds();
            foliageJob2.FinalizeSeeds();
            foliageJob3.FinalizeSeeds();
            Tree.Foliage foliage = new Tree.Foliage();
            foliage.RenderComps.Add(new RenderComponent(foliageJob, mat4.Identity));
            foliage.RenderComps.Add(new RenderComponent(foliageJob2, mat4.Identity));
            foliage.RenderComps.Add(new RenderComponent(foliageJob3, mat4.Identity));
            t.Leaves.Add(foliage);

            return t;
        }
    }
}

