﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Field;
using Xunit;

namespace Sleepey.FF8ModTest
{
    public class FieldScriptTest
    {
        [Theory]
        [InlineData(EntityType.Line, 3, 61)]
        [InlineData(EntityType.Background, 19, 0)]
        [InlineData(EntityType.Other, 120, 300)]
        public void EntityInfoTest(EntityType type, int count, int label)
        {
            // encode & decode
            var entityInfo = new EntityInfo(type, count, label);
            entityInfo = new EntityInfo(type, entityInfo.Encode());

            // make sure nothing's changed
            Assert.Equal(type, entityInfo.Type);
            Assert.Equal(count, entityInfo.ScriptCount);
            Assert.Equal(label, entityInfo.Label);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(356, 0)]
        [InlineData(6284, 1)]
        public void ScriptInfoTest(int position, int flag)
        {
            // encode & decode
            var scriptInfo = new ScriptInfo(position, flag);
            scriptInfo = new ScriptInfo(scriptInfo.Encode());

            // make sure nothing's changed
            Assert.Equal(position, scriptInfo.Position);
            Assert.Equal(flag, scriptInfo.Flag);
        }

        [Theory]
        [InlineData(0, EntityType.Door)]
        [InlineData(47, EntityType.Other)]
        [InlineData(120, EntityType.Line)]
        public void EntityTest(int label, EntityType type)
        {
            // construct a simple entity
            var labelInstr = new FieldScriptInstruction(5, label);
            var script = new Script(new List<FieldScriptInstruction>() { labelInstr }, false);
            var entity = new Entity(type, new List<Script>() { script });

            // extract info, then encode & decode
            var entityInfo = new EntityInfo(entity);
            entityInfo = new EntityInfo(entity.Type, entityInfo.Encode());

            // info reflects the original entity
            Assert.Equal(label, entityInfo.Label);
            Assert.Equal(type, entityInfo.Type);
            Assert.Equal(0, entityInfo.ScriptCount);
        }

        [Theory]
        [InlineData(0x05, 0)]
        [InlineData(0x159, -1)]
        [InlineData(0x0a, 1039)]
        [InlineData(0x07, 16384)]
        [InlineData(0x07, -129)]
        public void InstructionTest(int opcode, int param)
        {
            // encode & decode
            var instruction = new FieldScriptInstruction(opcode, param);
            if (param == -1) instruction = new FieldScriptInstruction(opcode);
            instruction = new FieldScriptInstruction(BitConverter.GetBytes(instruction.Encode()));

            // make sure nothing's changed
            Assert.Equal(opcode, instruction.OpCode);
            if (param == -1)
            {
                Assert.False(instruction.HasParam);
            }
            else
            {
                Assert.True(instruction.HasParam);
                Assert.Equal(param, instruction.Param);
            }
        }

        [Fact]
        public void FullTest()
        {
            // load a real jsm file from the game
            var jsmFile = File.ReadAllBytes(@"TestData\gggate4.jsm");
            var script = FieldScript.FromBytes(jsmFile);

            // 3 entities, all "other" type
            Assert.Equal(3, script.Entities.Count);
            Assert.Equal(3, script.Entities.Where(e => e.Type == EntityType.Other).Count());

            // 4 scripts each
            Assert.Equal(4, script.Entities[0].Scripts.Count);
            Assert.Equal(4, script.Entities[1].Scripts.Count);
            Assert.Equal(4, script.Entities[2].Scripts.Count);

            // spot check some scripts & instructions
            Assert.Equal(8, script.Entities[0].Scripts[0].Instructions.Count);
            Assert.Equal(3, script.Entities[1].Scripts[1].Instructions.Count);
            Assert.Equal(15, script.Entities[2].Scripts[1].Instructions.Count);
            Assert.Equal(5, script.Entities[1].Scripts[2].Instructions[0].OpCode);
            Assert.Equal(6, script.Entities[1].Scripts[2].Instructions[0].Param);
            Assert.Equal(6, script.Entities[1].Scripts[2].Instructions[1].OpCode);
            Assert.Equal(8, script.Entities[1].Scripts[2].Instructions[1].Param);

            // re-encode & run the same tests again
            script = FieldScript.FromBytes(script.Encode());

            Assert.Equal(3, script.Entities.Count);
            Assert.Equal(3, script.Entities.Where(e => e.Type == EntityType.Other).Count());

            Assert.Equal(4, script.Entities[0].Scripts.Count);
            Assert.Equal(4, script.Entities[1].Scripts.Count);
            Assert.Equal(4, script.Entities[2].Scripts.Count);

            Assert.Equal(8, script.Entities[0].Scripts[0].Instructions.Count);
            Assert.Equal(3, script.Entities[1].Scripts[1].Instructions.Count);
            Assert.Equal(15, script.Entities[2].Scripts[1].Instructions.Count);
            Assert.Equal(5, script.Entities[1].Scripts[2].Instructions[0].OpCode);
            Assert.Equal(6, script.Entities[1].Scripts[2].Instructions[0].Param);
            Assert.Equal(6, script.Entities[1].Scripts[2].Instructions[1].OpCode);
            Assert.Equal(8, script.Entities[1].Scripts[2].Instructions[1].Param);
        }

        [Fact]
        public void FullTest2()
        {
            // load a real jsm file from the game
            var jsmFile = File.ReadAllBytes(@"TestData\bdifrit1.jsm");
            var script = FieldScript.FromBytes(jsmFile);

            // 16 entities with all the correct scripts
            Assert.Equal(16, script.Entities.Count);
            Assert.Single(script.Entities.Where(e => e.Type == EntityType.Line).ToList());
            Assert.Empty(script.Entities.Where(e => e.Type == EntityType.Door).ToList());
            Assert.Equal(4, script.Entities.Where(e => e.Type == EntityType.Background).Count());
            Assert.Equal(11, script.Entities.Where(e => e.Type == EntityType.Other).Count());

            Assert.Equal(8, script.Entities[0].Scripts.Count);
            Assert.Equal(44, script.Entities[0].Scripts[0].Instructions[0].Param);
            Assert.Equal(2, script.Entities[1].Scripts.Count);
            Assert.Equal(56, script.Entities[1].Scripts[0].Instructions[0].Param);
            Assert.Equal(4, script.Entities[15].Scripts.Count);
            Assert.Equal(52, script.Entities[15].Scripts[0].Instructions[0].Param);

            // re-encode & run the same tests again
            script = FieldScript.FromBytes(script.Encode());

            Assert.Equal(16, script.Entities.Count);
            Assert.Single(script.Entities.Where(e => e.Type == EntityType.Line).ToList());
            Assert.Empty(script.Entities.Where(e => e.Type == EntityType.Door).ToList());
            Assert.Equal(4, script.Entities.Where(e => e.Type == EntityType.Background).Count());
            Assert.Equal(11, script.Entities.Where(e => e.Type == EntityType.Other).Count());

            Assert.Equal(8, script.Entities[0].Scripts.Count);
            Assert.Equal(44, script.Entities[0].Scripts[0].Instructions[0].Param);
            Assert.Equal(2, script.Entities[1].Scripts.Count);
            Assert.Equal(56, script.Entities[1].Scripts[0].Instructions[0].Param);
            Assert.Equal(4, script.Entities[15].Scripts.Count);
            Assert.Equal(52, script.Entities[15].Scripts[0].Instructions[0].Param);
        }
    }
}
