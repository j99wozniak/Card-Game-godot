using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;

public class Save
{
    public int tileSize;
    public int numberOfTeams;
    public int currentTeam;
    public int sizeX;
    public int sizeY;

    public int currentUnitID;
    public int currentUnitEffectID;
    public int currentTileEffectID;

    public List<UnitSave> units = new();
    public List<CustomTileSave> customTiles = new();
    public List<TileEffectSave> tileEffects = new();
    public string presetTiles;
    
    public class UnitSave{
        public int ID;
        public string unitName;
        public int team;
        public int baseMaxHp;
        public int currentHp;
        public int baseMaxStamina;
        public int currentStamina;
        public int baseMaxMovement;
        public int currentMovement;
        public bool isDead;
        public int x;
        public int y;
        public int unitSpriteFrames;
        public List<SkillSave> skillSaves = new();
        public List<UnitEffectSave> unitEffects = new();
        public class SkillSave{
            public int sourceID;
            public string name;
            public int type;
            public int category;
            public bool isMelee;
            public int basePower;
            public int baseCost;
            public int baseRange;
        }
        public class UnitEffectSave{
            public int ID;
            public string name;
            public int parentUnitID;
            public int sourceID;
            public int count;
            public int priority;
            public int power;
            public bool stackable;
            public bool removedOnDeath;
            public int type;
            public int trigger;
            public int countdownTrigger;
            public List<int> linkedTileEffectsIDs = new();
            public List<int> linkedUnitEffectsIDs = new();
        }
    }
    
    public class CustomTileSave{
        public int ID;
        public string tileName;
        public float cost;
        public int tileTexture;
        //TODO might want to have an overload for custom textures. Same for units
    }

    public class TileEffectSave{
        public int ID;
        public string name;
        public int parentTileID;
        public int sourceID;
        public int count;
        public int priority;
        public int power;
        public bool stackable;
        public int type;
        public int trigger;
        public int countdownTrigger;
        public List<int> linkedTileEffectsIDs = new();
        public List<int> linkedUnitEffectsIDs = new();
    }
}

public static class SaveUtil
{
    static JsonSerializerOptions options = new JsonSerializerOptions{
        IncludeFields = true, 
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault};

    public static Save CreateSave(Game game){
        Save save = new Save
        {
            tileSize = Game.tileSize,
            numberOfTeams = game.numberOfTeams,
            currentTeam = game.currentTeam,
            sizeX = game.map.sizeX,
            sizeY = game.map.sizeY,
            currentUnitID = Unit.currentUnitID,
            currentUnitEffectID = UnitEffect.currentUnitEffectID,
            currentTileEffectID = TileEffect.currentTileEffectID
        };

        StringBuilder tempPresetTiles = new StringBuilder();

        for (int i = 0; i < game.map.sizeX; i++){
            for (int j = 0; j < game.map.sizeY; j++){
                Unit currentUnit = game.map.unitMap[i,j];
                if(currentUnit!=null){
                    Save.UnitSave unitSave = new(){
                        ID = currentUnit.ID,
                        unitName = currentUnit.unitName,
                        team = currentUnit.team,
                        baseMaxHp = currentUnit.baseMaxHp,
                        currentHp = currentUnit.currentHp,
                        baseMaxStamina = currentUnit.baseMaxStamina,
                        currentStamina = currentUnit.currentStamina,
                        baseMaxMovement = currentUnit.baseMaxStamina,
                        currentMovement = currentUnit.currentMovement,
                        isDead = currentUnit.isDead,
                        x = currentUnit.x,
                        y = currentUnit.y,
                        unitSpriteFrames = (int)currentUnit.unitSpriteFrames
                    };

                    foreach(Skill s in currentUnit.skills){
                        Save.UnitSave.SkillSave skillSave = new(){
                            sourceID = s.source?.ID ?? 0,
                            name = s.name,
                            type = (int) s.type,
                            category = (int) s.category,
                            isMelee = s.isMelee,
                            basePower = s.basePower,
                            baseCost = s.baseCost,
                            baseRange = s.baseRange
                        };
                        unitSave.skillSaves.Add(skillSave);
                    }

                    foreach (KeyValuePair<Trigger, LinkedList<UnitEffect>> list in currentUnit.unitEffects){
                        foreach(UnitEffect e in list.Value){
                            Save.UnitSave.UnitEffectSave unitEffectSave = new(){
                                ID = e.ID,
                                name = e.name,
                                parentUnitID = e.parentUnit.ID,
                                sourceID = e.source?.ID ?? 0,
                                count = e.count,
                                priority = e.priority,
                                power = e.power,
                                stackable = e.stackable,
                                removedOnDeath = e.removedOnDeath,
                                type = (int)e.type,
                                trigger = (int)e.trigger,
                                countdownTrigger = (int)e.countdownTrigger
                            };
                            foreach (UnitEffect lue in e.linkedUnitEffects){
                                unitEffectSave.linkedUnitEffectsIDs.Add(lue.ID);
                            }
                            foreach(TileEffect lte in e.linkedTileEffects){
                                unitEffectSave.linkedTileEffectsIDs.Add(lte.ID);
                            }
                            unitSave.unitEffects.Add(unitEffectSave);
                        }
                    }
                    save.units.Add(unitSave);
                }

                Tile currentTile = game.map.tileMap[i,j];
                if(currentTile.tilePreset == TilePreset.none){
                    Save.CustomTileSave customTile = new(){
                        ID = currentTile.ID,
                        tileName = currentTile.tileName,
                        cost = currentTile.cost,
                        tileTexture = (int)currentTile.tileTexture
                    };
                    save.customTiles.Add(customTile);
                }
                else{
                    tempPresetTiles.Append($"{currentTile.ID}:{(int)currentTile.tilePreset}, ");
                }
                foreach(TileEffect e in currentTile.tileEffects){
                    Save.TileEffectSave tileEffectSave = new(){
                        ID = e.ID,
                        name = e.name,
                        parentTileID = e.parentTile.ID,
                        sourceID = e.source?.ID ?? 0,
                        count = e.count,
                        priority = e.priority,
                        power = e.power,
                        stackable = e.stackable,
                        type = (int)e.type,
                        trigger = (int)e.trigger,
                        countdownTrigger = (int)e.countdownTrigger
                    };
                    foreach (UnitEffect lue in e.linkedUnitEffects){
                        tileEffectSave.linkedUnitEffectsIDs.Add(lue.ID);
                    }
                    foreach(TileEffect lte in e.linkedTileEffects){
                        tileEffectSave.linkedTileEffectsIDs.Add(lte.ID);
                    }
                    save.tileEffects.Add(tileEffectSave);
                }
            } 
        }
        save.presetTiles = tempPresetTiles.ToString();
        return save;
    }
    public static string SaveGame(Game game){
        Save save = CreateSave(game);
        string saveJson = JsonSerializer.Serialize<Save>(save, options);
        using(var file = FileAccess.Open("user://save_game.json", FileAccess.ModeFlags.Write)){
            file.StoreString(saveJson);
        }
        return saveJson;
    }

    public static void CreateGame(Game game, Save save){
        Dictionary<int, Unit> lookupUnits = new();
        Dictionary<int, UnitEffect> lookupUnitEffects = new();
        Dictionary<int, TileEffect> lookupTileEffects = new();

        Game.tileSize = save.tileSize;
        GameMap map = new GameMap(save.sizeX, save.sizeY);

        Func<int, (int x, int y)> toXY = ID => (ID / map.maxSize, ID % map.maxSize);
        string[] presetTiles = save.presetTiles.Split(',', StringSplitOptions.TrimEntries);
        foreach(string t in presetTiles){
            if(!string.IsNullOrWhiteSpace(t)){
                (int ID, TilePreset tilePreset) = t.Split(":") switch {
                    var tab => (int.Parse(tab[0]), (TilePreset)int.Parse(tab[1]))};
                Tile newPresetTile = Factory.GetPresetTile(tilePreset, ID, map);
                map.AddChild(Tile.createTileNode(newPresetTile));
            }
        }
        foreach(Save.CustomTileSave t in save.customTiles){
            (int x, int y) = toXY(t.ID);
            Tile newCustom = new Tile(map, t.tileName, t.cost, x, y, (TileTexture) t.tileTexture);
            map.AddChild(Tile.createTileNode(newCustom));
        }

        foreach(Save.UnitSave u in save.units){
            Unit newUnit = new Unit(map, u.unitName, 
                u.team, u.baseMaxHp, u.baseMaxStamina, 
                u.baseMaxMovement, u.x, u.y, 
                (UnitSpriteFrames) u.unitSpriteFrames, u.isDead);
            newUnit.ID = u.ID;
            newUnit.currentHp = u.currentHp;
            newUnit.currentMovement = u.currentMovement;
            newUnit.currentStamina = u.currentStamina;
            if(u.isDead){
                map.graveyard.Add(newUnit);
            }
            lookupUnits.Add(u.ID, newUnit);
            SpriteFrames uSpriteFrames = Factory.GetUnitSpriteFrames(newUnit.unitSpriteFrames);
            map.AddChild(Unit.createUnitNode(newUnit, uSpriteFrames));
            
            foreach(Save.UnitSave.UnitEffectSave ue in u.unitEffects){
                UnitEffect newEffect = Factory.GetUnitEffect(ue.name);
                newEffect.ID = ue.ID;
                newEffect.parentUnit = newUnit;
                newEffect.count = ue.count;
                newEffect.priority = ue.priority;
                newEffect.power = ue.power;
                newEffect.stackable = ue.stackable;
                newEffect.removedOnDeath = ue.removedOnDeath;
                newEffect.type = (Type) ue.type;
                newEffect.trigger = (Trigger) ue.trigger;
                newEffect.countdownTrigger = (Trigger) ue.countdownTrigger;
                lookupUnitEffects.Add(ue.ID, newEffect);
                newUnit.AddUnitEffect(newEffect);
            }
        }
        foreach(Save.TileEffectSave te in save.tileEffects){
            TileEffect newEffect = Factory.GetTileEffect(te.name);
            (int x, int y) = toXY(te.parentTileID);
            newEffect.ID = te.ID;
            newEffect.parentTile = map.tileMap[x, y];
            newEffect.count = te.count;
            newEffect.priority = te.priority;
            newEffect.power = te.power;
            newEffect.stackable = te.stackable;
            newEffect.type = (Type) te.type;
            newEffect.trigger = (Trigger) te.trigger;
            newEffect.countdownTrigger = (Trigger) te.countdownTrigger;
            lookupTileEffects.Add(te.ID, newEffect);
            newEffect.parentTile.AddTileEffect(newEffect);
        }

        foreach(Save.UnitSave u in save.units){
            Unit currentUnit = lookupUnits[u.ID];
            foreach(Save.UnitSave.SkillSave s in u.skillSaves){
                Skill newSkill = Factory.GetSkill(s.name);
                newSkill.source = lookupUnits?.GetValueOrDefault(s.sourceID);
                newSkill.name = s.name;
                newSkill.type = (Type) s.type;
                newSkill.category = (Category) s.category;
                newSkill.isMelee = s.isMelee;
                newSkill.basePower = s.basePower;
                newSkill.baseCost = s.baseCost;
                newSkill.baseRange = s.baseRange;
                currentUnit.AddSkill(newSkill);
            }
            foreach(Save.UnitSave.UnitEffectSave ue in u.unitEffects){
                UnitEffect currentEffect = lookupUnitEffects[ue.ID];
                currentEffect.source = lookupUnits?.GetValueOrDefault(ue.sourceID);

                foreach(int lueID in ue.linkedUnitEffectsIDs){
                    currentEffect.linkedUnitEffects.AddLast(lookupUnitEffects[lueID]);
                }
                foreach(int lteID in ue.linkedTileEffectsIDs){
                    currentEffect.linkedTileEffects.AddLast(lookupTileEffects[lteID]);
                }
            }
        }
        foreach(Save.TileEffectSave te in save.tileEffects){
                TileEffect currentEffect = lookupTileEffects[te.ID];
                currentEffect.source = lookupUnits?.GetValueOrDefault(te.sourceID);

                foreach(int lueID in te.linkedUnitEffectsIDs){
                    currentEffect.linkedUnitEffects.AddLast(lookupUnitEffects[lueID]);
                }
                foreach(int lteID in te.linkedTileEffectsIDs){
                    currentEffect.linkedTileEffects.AddLast(lookupTileEffects[lteID]);
                }
            }
        // Set the rest of variables
        game.numberOfTeams = save.numberOfTeams;
        game.currentTeam = save.currentTeam;
        Unit.currentUnitID = save.currentUnitID;
        UnitEffect.currentUnitEffectID = save.currentUnitEffectID;
        TileEffect.currentTileEffectID = save.currentTileEffectID;
        if(game.map != null){
            game.map.QueueFree();
        }
        game.map = map;
        game.AddChild(map);
    }
    public static Save LoadSave(){
        using var file = FileAccess.Open("user://save_game.json", FileAccess.ModeFlags.Read);
        string loadJson = file.GetAsText();
        Save loadedSave = JsonSerializer.Deserialize<Save>(loadJson, options);
        return loadedSave;
    }
}
