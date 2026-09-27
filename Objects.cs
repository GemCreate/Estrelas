using Lua;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Estrelas
{
    [LuaObject]
    public partial class LuaButton
    {

        public LuaButton(string _name, string _id, int _width, int _height)
        {
            this.name = _name;
            this.id = _id;
            this.width = _width;
            this.height = _height;

        }
  
        // TODO: Implement x and y coords instead of flow layout


        [LuaMember("id")]
        public string id { get; set; } = "";

        [LuaMember("name")]
        public string name { get; set; } = "";

        [LuaMember("width")]
      public int width { get; set; } = 0;


        [LuaMember("height")]
        public int height { get; set; } = 0;

        public Button btn { get; set; }

    }



    }
