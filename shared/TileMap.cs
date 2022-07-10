using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TileMap : Godot.TileMap
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		FixTileAdjacency();
	}
	private string GetAdjacency(Vector2 tile)
	{
		int tile_x=(int)tile.x;
		int tile_y=(int)tile.y;
		var int_array=new int[] {
			(int)GetCell(tile_x-1,tile_y-1),
			(int)GetCell(tile_x,tile_y-1),
			(int)GetCell(tile_x+1,tile_y-1),
			(int)GetCell(tile_x-1,tile_y),
			(int)GetCell(tile_x+1,tile_y),
			(int)GetCell(tile_x-1,tile_y+1),
			(int)GetCell(tile_x,tile_y+1),
			(int)GetCell(tile_x+1,tile_y+1),
		};
		string tile_map=string.Empty;
		foreach (int i in int_array)
		{
			tile_map+=i.ToString()+",";
		}
		return tile_map;
	}
	private void FixTileAdjacency()
	{
		var tiles=new List<(int t,int x,int y,string a)>();
		{
			var tile_map=GetUsedCells();
			foreach (Vector2 tile in tile_map)
			{
				var tile_adj=GetAdjacency(tile);
				int tile_x=(int)tile.x;
				int tile_y=(int)tile.y;
				int tile_type=GetCell((int)tile.x,(int)tile.y);
				tiles.Add((tile_type,tile_x,tile_y,tile_adj));
			}
		}
		foreach ((int t,int x,int y, string a) in tiles)
		{
			switch (t)
			{
				case 0:
				case 1:
					switch (true)
					{
						case bool _ when Regex.IsMatch(a,@"3,3,\-?\d+,3,[01],\-?\d+,\-?\d+,2,"):
							SetCell(x,y,5);
							break;
						case bool _ when Regex.IsMatch(a,@"\-?\d+,3,3,[01],3,2,[01],\-?\d+,"):
							SetCell(x,y,6);
							break;
						case bool _ when Regex.IsMatch(a,@"\-?\d+,[01],2,3,[01],3,3,\-?\d+,"):
							SetCell(x,y,7);
							break;
						case bool _ when Regex.IsMatch(a,@"2,[01],\-?\d+,[01],3,\-?\d+,3,3,"):
							SetCell(x,y,8);
							break;
						case bool _ when Regex.IsMatch(a,@"\-?\d+,[01],[012],3,2,\-?\d+,[01],[012],"):
							SetCell(x,y,9);
							break;
						case bool _ when Regex.IsMatch(a,@"[012],2,[012],[01],[01],\-?\d+,3,\-?\d+,"):
							SetCell(x,y,10);
							break;
						case bool _ when Regex.IsMatch(a,@"\-?\d+,3,\-?\d+,[01],[01],[012],2,[012]"):
							SetCell(x,y,11);
							break;
						case bool _ when Regex.IsMatch(a,@"[012],[01],\-?\d+,2,3,[012],[01],\-?\d+,"):
							SetCell(x,y,12);
							break;
						case bool _ when Regex.IsMatch(a,@"\-?\d+,[01],[01],3,[01],\-?\d+,[01],2,"):
							SetCell(x,y,13);
							break;
						case bool _ when Regex.IsMatch(a,@"[01],[01],\-?\d+,[01],3,2,[01],\-?\d+,"):
							SetCell(x,y,14);
							break;
						case bool _ when Regex.IsMatch(a,@"\-?\d+,[01],2,3,[01],\-?\d+,[01],[01],"):
							SetCell(x,y,15);
							break;
						case bool _ when Regex.IsMatch(a,@"2,[01],\-?\d+,[01],3,[01],[01],\-?\d+,"):
							SetCell(x,y,16);
							break;
						case bool _ when Regex.IsMatch(a,@"[01],[01],3,3,[01],[01],[01],2"):
							SetCell(x,y,17);
							break;
						case bool _ when Regex.IsMatch(a,@"3,[01],[01],[01],3,2,[01],[01],"):
							SetCell(x,y,18);
							break;
						case bool _ when Regex.IsMatch(a,@"[01],[01],2,3,[01],[01],[01],3,"):
							SetCell(x,y,19);
							break;
						case bool _ when Regex.IsMatch(a,@"2,[01],[01],[01],3,3,[01],[01],"):
							SetCell(x,y,20);
							break;
						case bool _ when Regex.IsMatch(a,@"3,[01],[01],[01],2,[01],[01],[012],"):
							SetCell(x,y,21);
							break;
						case bool _ when Regex.IsMatch(a,@"[01],[01],3,[01],[01],[012],2,[01],"):
							SetCell(x,y,22);
							break;
						case bool _ when Regex.IsMatch(a,@"[01],2,[012],[01],[01],3,[01],[01],"):
							SetCell(x,y,23);
							break;
						case bool _ when Regex.IsMatch(a,@"[012],[01],[01],2,[01],[01],[01],3,"):
							SetCell(x,y,24);
							break;
						case bool _ when Regex.IsMatch(a,@"3,[01],[012],[01],2,3,[01],[012]"):
							SetCell(x,y,29);
							break;
						case bool _ when Regex.IsMatch(a,@"3,[01],3,[01],[01],[012],2,[012],"):
							SetCell(x,y,30);
							break;
						case bool _ when Regex.IsMatch(a,@"[012],2,[012],[01],[01],3,[01],3,"):
							SetCell(x,y,31);
							break;
						case bool _ when Regex.IsMatch(a,@"[012],[01],3,2,[01],[012],[01],3"):
							SetCell(x,y,32);
							break;
						case bool _ when Regex.IsMatch(a,@"2,2,\-?\d+,2,[01],\-?\d+,[01],3,"):
							SetCell(x,y,33);
							break;
						case bool _ when Regex.IsMatch(a,@"\-?\d+,2,2,[01],2,3,[01],\-?\d+,"):
							SetCell(x,y,34);
							break;
						case bool _ when Regex.IsMatch(a,@"\-?\d+,[01],3,2,[01],2,2,\-?\d+,"):
							SetCell(x,y,35);
							break;
						case bool _ when Regex.IsMatch(a,@"3,[01],\-?\d+,[01],2,\-?\d+,2,2,"):
							SetCell(x,y,36);
							break;
						case bool _ when Regex.IsMatch(a,@"[01],[01],2,[01],[01],\-?\d+,3,\-?\d+,"):
							SetCell(x,y,13,false,true,true);
							break;
						case bool _ when Regex.IsMatch(a,@"2,[01],[01],[01],[01],\-?\d+,3,\-?\d+,"):
							SetCell(x,y,14,true,false,true);
							break;
						case bool _ when Regex.IsMatch(a,@"\-?\d+,3,\-?\d+,[01],[01],[01],[01],2,"):
							SetCell(x,y,15,true,false,true);
							break;
						case bool _ when Regex.IsMatch(a,@"\-?\d+,3,\-?\d+,[01],[01],2,[01],[01],"):
							SetCell(x,y,16,false,true,true);
							break;
						case bool _ when Regex.IsMatch(a,@"3,[01],[012],[01],[01],[012],2,[012],"):
							SetCell(x,y,21,false,false,true);
							break;
						case bool _ when Regex.IsMatch(a,@"[01],[01],[012],[01],2,3,[01],[012],"):
							SetCell(x,y,22,false,false,true);
							break;
						case bool _ when Regex.IsMatch(a,@"[012],[01],3,2,[01],[012],[01],[01],"):
							SetCell(x,y,23,false,false,true);
							break;
						case bool _ when Regex.IsMatch(a,@"[012],2,[012],[01],[01],[012],[01],3,"):
							SetCell(x,y,24,false,false,true);
							break;
					}
					break;
				case 2:
					switch (true)
					{
						case bool _ when Regex.IsMatch(a,@"2,2,\-?\d+,2,\-?\d+,\-?\d+,\-?\d+,\-?\d+,"):
						case bool _ when Regex.IsMatch(a,@"\-?\d+,2,2,\-?\d+,2,\-?\d+,\-?\d+,\-?\d+,"):
						case bool _ when Regex.IsMatch(a,@"\-?\d+,\-?\d+,\-?\d+,2,\-?\d+,2,2,\-?\d+,"):
						case bool _ when Regex.IsMatch(a,@"\-?\d+,\-?\d+,\-?\d+,\-?\d+,2,\-?\d+,2,2,"):
							break;
						default:
							SetCell(x,y,4);
							break;
					}
				break;
			}
		}
	}
}
