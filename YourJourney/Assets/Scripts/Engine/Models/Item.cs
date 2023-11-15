using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum Slot { NONE, ARMOR, HAND, TRINKET, MOUNT };

public enum ItemSeries
{
    NONE = 0,

    //Armor
    CLOAK = 1,
    HOARY_COAT = 2,
    PADDED_ARMOR = 3,
    PLATE_ARMOR = 4,
    RING_MAIL = 5,
    TRAVEL_GARB = 6,

    //Support
    BANNER = 100,
    HARP = 101,
    HORN = 102,
    SHIELD = 103,

    //Weapons
    BATTLE_AXE = 200,
    DAGGER = 201,
    GREAT_BOW = 202,
    HAMMER = 203,
    HATCHET = 204,
    KNIFE = 205,
    MACE = 206,
    RENDING_CLAWS = 207,
    SHORT_BOW = 208,
    SHORT_SWORD = 209,
    SLING = 210,
    SPEAR = 211,
    STAFF = 212,
    SWORD = 213,
    WALKING_STICK = 214,

    //Trinkets
    BOOTS = 300,
    BROOCH = 301,
    CIRCLET = 302,
    EXTRA_RATIONS = 303,
    FANG_PENDANT = 304,
    HAMMER_AND_TONGS = 305,
    HANDKERCHIEF = 306,
    HELMET = 307,
    OLD_MAP = 308,
    OLD_PIPE = 309,
    OLD_SCEPTER = 310,
    PROVISIONS = 311,
    ROPE = 312,
    THE_CROWN_OF_SHADOWS = 313,
    TOME = 314,
    TORCH = 315,
    WATERSKIN = 316,

    //Mounts
    FRIENDLY_PONY = 400,
    GRUMBLE_BUM = 401,
    MEADOW_HART = 402,
    PACK_MULE = 403,
    QUICKBEAM = 404,
    SNOWBRIGHT = 405,
    SWIFT_STEED = 406,
    TRAVELLERS_HORSE = 407,
    WAR_CHARGER = 408,
    WITNESS_OF_MARANWE = 409
};

public class Item
{
    public int id;
    public int collection;
    public Slot slotId;
    public string slot;
    public ItemSeries seriesId;
    public string seriesName;
    public string dataName;
    public string originalName;
    public int count;
    public int tier;
    public string[] stats;
    public string trait;
    public int upgrade;
    public int tokens;
    public int handed;
    public int ranged;

    public Item() { }

    public Item(int id)
    {
        this.id = id;
    }
}
