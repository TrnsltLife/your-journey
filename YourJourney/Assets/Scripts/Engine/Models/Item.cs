using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ItemType { NONE, ARMOR, HAND, TRINKET, MOUNT };

public class Item
{
    public int id;
    public int collection;
    public ItemType typeId;
    public string type;
    public string item;
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
