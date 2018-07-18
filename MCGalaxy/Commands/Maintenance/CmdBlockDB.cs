﻿/*
    Copyright 2011 MCForge
    
    Dual-licensed under the    Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using MCGalaxy.SQL;

namespace MCGalaxy.Commands.Maintenance {
    public sealed class CmdBlockDB : Command2 {
        public override string name { get { return "BlockDB"; } }
        public override string type { get { return CommandTypes.World; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override CommandAlias[] Aliases {
            get { return new [] { new CommandAlias("ClearBlockChanges", "clear"),
                    new CommandAlias("cbc", "clear") }; }
        }
        public override bool MessageBlockRestricted { get { return true; } }
        
        public override void Use(Player p, string message, CommandData data) {
            string[] args = message.SplitSpaces();
            if (args.Length == 1 && p.IsSuper) { SuperRequiresArgs(p, "map name"); return; }
            
            Level lvl = p.IsSuper ? null : p.level;
            if (args.Length > 1) {
                lvl = Matcher.FindLevels(p, args[1]);
                if (lvl == null) return;
            }
            if (!LevelInfo.ValidateAction(p, data.Rank, lvl, "change BlockDB state of this level")) return;
            
            if (args[0].CaselessEq("clear")) {
                p.Message("Clearing &cALL %Sblock changes for {0}%S...", lvl.ColoredName);
                if (Database.TableExists("Block" + lvl.name))
                    Database.Backend.DeleteTable("Block" + lvl.name);
                lvl.BlockDB.DeleteBackingFile();
                p.Message("Cleared &cALL %Sblock changes for " + lvl.ColoredName);
            } else if (args[0].CaselessEq("disable")) {
                lvl.Config.UseBlockDB = false;
                lvl.BlockDB.Cache.Enabled = false;
                
                p.Message("&cDisabled %Srecording further block changes for " + lvl.ColoredName);
                Level.SaveSettings(lvl);
            } else if (args[0].CaselessEq("enable")) {
                lvl.Config.UseBlockDB = true;
                lvl.BlockDB.Cache.Enabled = true;
                
                p.Message("&aEnabled %Srecording further block changes for " + lvl.ColoredName);
                Level.SaveSettings(lvl);
            } else {
                Help(p);
            }
        }
        
        public override void Help(Player p) {
            p.Message("%T/BlockDB clear [map]");
            p.Message("%HClears the BlockDB (block changes stored in /about) for [map]");
            p.Message("%T/BlockDB disable [map]");
            p.Message("%HDisables recording block changes to the BlockDB for [map]");
            p.Message("%T/BlockDB enable [map]");
            p.Message("%HEnables %Hrecording block changes to the BlockDB for [map]");
            p.Message("%HIf no map name is given, uses your current map.");
            p.Message("%WUse these commands with great caution!");
        }
    }
}