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

        public LuaButton(string _name, string? _id,int _fnt, int _width, int _height)
        {
            this.name = _name;
            this.id = _id;
            this.width = _width;
            this.height = _height;
            this.fontScale = _fnt;
        }
  
        // TODO: Implement x and y coords instead of flow layout
        // Nah fuck that who cares!


        [LuaMember("id")]
        public string? id { get; set; } = "";

        [LuaMember("name")]
        public string name { get; set; } = "";

        [LuaMember("font_scale")]
        public int fontScale { get; set; } = 0;


        [LuaMember("width")]
      public int width { get; set; } = 0;


        [LuaMember("height")]
        public int height { get; set; } = 0;

        [LuaMember("visible")]
        public bool visible { get; set; } = true;

        public Button btn { get; set; }

    }

    [LuaObject]
    public partial class LuaLabel
    {

        public LuaLabel(string _name, string? _id, int _fnt)
        {
            this.name = _name;
            this.id = _id;
            this.fontScale = _fnt;


        }

       


        [LuaMember("id")]
        public string? id { get; set; } = "";

        [LuaMember("name")]
        public string name { get; set; } = "";

        [LuaMember("font_scale")]
        public int fontScale { get; set; } = 0;


        [LuaMember("visible")]
        public bool visible { get; set; } = true;

        public Label lbl { get; set; }

    }

    [LuaObject]
    public partial class LTxtInput
    {

        public LTxtInput(string _name, string? _id, int _fnt, int _width, int _height)
        {
            this.name = _name;
            this.id = _id;
            this.fontScale = _fnt;
            this.width = _width;
            this.height = _height;


        }

        


        [LuaMember("id")]
        public string? id { get; set; } = "";

        [LuaMember("name")]
        public string name { get; set; } = "";

        [LuaMember("font_scale")]
        public int fontScale { get; set; } = 0;


        [LuaMember("width")]
        public int width { get; set; } = 0;

        [LuaMember("height")]
        public int height { get; set; } = 0;

        [LuaMember("visible")]
        public bool visible { get; set; } = true;
        public TextBox txt { get; set; }

    }

    [LuaObject]
    public partial class LDivider
    {

        public LDivider( string? _id)
        {
       
            this.id = _id;
 

        }




        [LuaMember("id")]
        public string? id { get; set; } = "";

        [LuaMember("visible")]
        public bool visible { get; set; } = true;


        public PictureBox pix { get; set; }

    }

}
