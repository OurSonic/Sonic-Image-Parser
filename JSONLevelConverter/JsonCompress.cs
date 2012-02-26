using System;
using System.Collections.Generic;

namespace JSONLevelConverter
{
    public class JsonCompress
    {
        public JsonCompress()
        {
            
        }
        private Node process(Node root, string value)
        {
            return null;
        }


        /* 
    function process( root, value )
    {
        var result;
        var i;
        var key;
        var node;

        if ( typeof value === "object" ) {
            // if it's an array,
            if (Object.prototype.toString.apply(value) === '[object Array]') {
                // process each item in the array.
                result = [];
                for( i = 0; i < value.length; i++ ) {
                    result.push( process( root, value[i] ) ); 
                }
            } else {
                node = root;
                result = { "":[] };
                // it's an object. For each key,
                for (key in value) {
                    if ( Object.hasOwnProperty.call( value, key ) ) {
                        // follow the node.
                        node = node.follow( key );

                        // add its value to the array.
                        result[""].push( process( root, value[key] ) );
                    }
                }

                node.links.push( result );
            }
        } else {
            result = value;
        }

        return result;
    }
    }*/

        public string Compress(string txt)
        {
            var root = new Node(null, "");
            var values = process(root, txt);
            //var templates = createTemplates(root);
            //if (templates.Count() > 0)
            {
                /*       return JSON.stringify( { "f": "cjson", "t": templates, 
                "v": values }, null, null );*/
            }
            //else 
            return txt;
        }
    }
    public class Node
    {
        public Node(Node parent, string key)
        {
            Parent = parent;
            Key = key;
            this.Children = new Dictionary<string, Node>();
            this.Links = new List<string>();
            this.TemplateIndex = 0;
        }

        protected int TemplateIndex { get; set; }

        protected Node Parent { get; set; }
        protected string Key { get; set; }
        protected Dictionary<string,Node> Children { get; set; }
        protected List<string> Links { get; set; }
        public Node Follow(string key)
        {
            if(Children.ContainsKey(key))
            {
                return Children[key];
            }else
            {
                Node n;
                Children.Add(key,n=new Node(this, key));
                return n;
            }
        }
    }
    /*

(function(){

    

    // Given the root of the key tree, process the value possibly adding to the
    // key tree.
    function process( root, value )
    {
        var result;
        var i;
        var key;
        var node;

        if ( typeof value === "object" ) {
            // if it's an array,
            if (Object.prototype.toString.apply(value) === '[object Array]') {
                // process each item in the array.
                result = [];
                for( i = 0; i < value.length; i++ ) {
                    result.push( process( root, value[i] ) ); 
                }
            } else {
                node = root;
                result = { "":[] };
                // it's an object. For each key,
                for (key in value) {
                    if ( Object.hasOwnProperty.call( value, key ) ) {
                        // follow the node.
                        node = node.follow( key );

                        // add its value to the array.
                        result[""].push( process( root, value[key] ) );
                    }
                }

                node.links.push( result );
            }
        } else {
            result = value;
        }

        return result;
    }

    // Given the root of the key tree, return the array of template arrays.
    function createTemplates( root )
    {
        var templates = [];
        var queue = [];
        var node;
        var template;
        var cur;
        var i;
        var key;
        var numChildren;

        root.templateIndex = 0;

        for ( key in root.children ) {
            if ( Object.hasOwnProperty.call( root.children, key ) ) {
                queue.push( root.children[key] );
            }
        }

        // while queue not empty
        while( queue.length > 0 ) {
            // remove a ode from the queue,
            node = queue.shift();
            numChildren = 0;

            // add its children to the queue.
            for ( key in node.children ) {
                if ( Object.hasOwnProperty.call( node.children, key ) ) {
                    queue.push( node.children[key] );
                    numChildren += 1;
                }
            }

            // if the node had more than one child, or it has links,
            if ( numChildren > 1 || node.links.length > 0 ) {
                template = [];
                cur = node;

                // follow the path up from the node until one with a template
                // id is reached.
                while( cur.templateIndex === null ) {
                    template.unshift( cur.key );
                    cur = cur.parent;
                }

                template.unshift( cur.templateIndex );

                templates.push( template );
                node.templateIndex = templates.length;

                for( i = 0; i < node.links.length; i++ ) {
                    node.links[i][""].unshift( node.templateIndex );
                }
            }
        }

        return templates;
    }

    function Compress( value )
    {
        var root, templates, values;

        root = new Node( null, "" );
        values = process( root, value );
        templates = createTemplates( root );

        if ( templates.length > 0 ) {
            return JSON.stringify( { "f": "cjson", "t": templates, 
                "v": values }, null, null );
        } else {
            // no templates, so no compression is possible.
            return JSON.stringify( value );
        }
    }

    function getKeys( templates, index )
    {
        var keys = [];

        console.log( templates );
        while( index > 0 ) {
            keys = templates[index-1].slice( 1 ).concat( keys );
            index = templates[index-1][0];
        }

        console.log( keys );
        return keys;
    }

    function expand( templates, value )
    {
        var result, i, key, keys;

        // if it's an array, then expand each element of the array.
        if ( typeof value === 'object' ) {
            if (Object.prototype.toString.apply(value) === '[object Array]') {
                result = [];
                for ( i = 0; i < value.length; i++ ) {
                    result.push( expand( templates, value[i] ) );
                }

            } else {
                // if it's an object, then recreate the keys from the template
                // and expand.
                result = {};
                keys = getKeys( templates, value[""][0] );
                for( i = 0; i < keys.length; i++ ) {
                    result[keys[i]] = expand( templates, value[""][i+1] );
                }
            }
        } else {
            result = value;
        }

        return result;
    }

    function Expand( str )
    {
        var value = JSON.parse( str );
        if ( typeof value !== "object" ||
             !("f" in value) ||
             value["f"] !== "cjson" )
        {
            // not in cjson format. Return as is.
            return value;
        }

        return expand( value["t"], value["v"] );
    }

    window.CJSON = {};
    window.CJSON.stringify = Compress;
    window.CJSON.parse = Expand;

})();
*/
}