﻿using UnityEngine;
using System.Collections;
using System;

public class MazeStructure {
	private MazeTool mazeTool;
	private bool[,,] data;
	private Point3 door;
	private Point3 key;
	
	public MazeStructure(MazeTool top, MazeTool bottom, MazeTool left, MazeTool right, MazeTool front, MazeTool back) {
		// initialize data
		this.mazeTool = top;
		data = new bool[2+mazeTool.walls.GetLength(0), 2+mazeTool.walls.GetLength(0), 2+mazeTool.walls.GetLength(1)];
		for (int i=0; i<data.GetLength(0); ++i)
			for (int j=0; j<data.GetLength(1); ++j)
				for (int k=0; k<data.GetLength(2); ++k)
					data[i, j, k] = true;
		
		// parse MazeTool walls, cells
		for (int i=0; i<mazeTool.walls.GetLength(0); ++i) {
			for (int j=0; j<mazeTool.walls.GetLength(1); ++j) {
				// parse walls
				if (mazeTool.walls[i, j]!=null) {
					data[1+i, 1, 1+j] = mazeTool.walls[i, j].gameObject.activeSelf;
					if (mazeTool.walls[i, j].type==MazeToolWall.WallType.door)
						door = new Point3(i+1, 1, j+1);
				}
				
				// parse cells
				else if (mazeTool.cells[i, j]!=null) {
					if (mazeTool.cells[i, j].type==MazeToolCell.CellType.key)
						key = new Point3(i+1, 1, j+1);
				}
			}
		}
		
		
		key = new Point3(9, 1, 9);
		door = new Point3(2, 1, 1);
		Debug.Log("key is at "+key);
		CreateWalls();
	}

	/// <summary>
	/// Returns a new GameObject that contains the 3D walls for the game
	/// </summary>
	public GameObject CreateWalls() {
		GameObject result = new GameObject("Walls");
		bool[,,] visited = new bool[data.GetLength(0), data.GetLength(1), data.GetLength(2)];
		visited.Initialize();
		int count=0;

		// iterate over {x, y, z}
		for (int axis=0; axis<3; ++axis) {
			// iterate over both sides of the given axis
			for (int side=1; side<data.GetLength(axis); side+=data.GetLength(axis)-3) {
				// create the parent object
				GameObject parent = new GameObject("bottom");
				parent.transform.parent = result.transform;
				//if (axis!=1 || side!=1)
					//continue;

				// iterate over all cells in the given axis
				for (int i=1; i<data.GetLength((axis+1)%3); i+=2) {
					for (int j=1; j<data.GetLength((axis+2)%3); j+=2) {
						// create a Point3 that corresponds to the current location
						Point3 p = Point3.zero;
						p[axis] = side;
						p[(axis+1)%3] = i;
						p[(axis+2)%3] = j;

						if (++count>1000)
							throw new Exception("count>200");

						// if the cell hasn't been created...
						if (!visited[p.x, p.y, p.z]) {
							Vector3 a = (p-1).ToVector3()/2;
							a[axis] = (side==1?(side-1)/2:(side+1)/2);
							Vector3 x = Vector3.zero;
							x[(axis+(side==1?1:2))%3] = 1;
							Vector3 y = Vector3.zero;
							//y[axis] = (side+1)/2-a[axis];
							y[axis] = (side==1?1:-1);
							Vector3 z = Vector3.zero;
							z[(axis+(side==1?2:1))%3] = 1;

							// create GameObject with a mesh for the given MazeCell
							GameObject cell = new GameObject("Maze Cell @"+p.ToString());
							cell.transform.parent = parent.transform;

							// create the floor
							{
								GameObject floor = new GameObject("floor");
								floor.transform.parent = cell.transform;
								int[] tris = new int[]{0, 1, 2, 3, 2, 1};
								Vector3[] verts = new Vector3[]{
									Vector3FromCubeToSphere(a, mazeTool.width, a, 50),
									Vector3FromCubeToSphere(a+x, mazeTool.width, a+x, 50),
									Vector3FromCubeToSphere(a+z, mazeTool.width, a+z, 50),
									Vector3FromCubeToSphere(a+x+z, mazeTool.width, a+x+z, 50)
								};
								Vector3 normal = Vector3.Cross(x, y);
								normal = Vector3.zero;
								Vector3[] normals = new Vector3[]{normal, normal, normal, normal};
								Vector2[] uvs = new Vector2[]{Vector2.zero, Vector2.right, Vector2.up, Vector2.one};
								AddMesh(floor, tris, verts, normals, uvs);
								//if (i==1 && j==1)
								//Debug.Log(verts[0]+"; "+verts[1]+"; "+verts[2]+"; "+verts[3]);
							}

							// create walls
							{
								GameObject walls = new GameObject("walls");
								walls.transform.parent = cell.transform;
								int[] tris = new int[]{0,1,4, 1,5,4, 1,2,5, 2,6,5, 2,3,6, 3,7,6, 3,0,7, 0,4,7,
									4,5,8, 5,9,8, 5,6,9, 6,10,9, 6,7,10, 7,11,10, 7,4,11, 4,8,11};
								y *= 2;
								Vector3[] verts = new Vector3[]{
									Vector3FromCubeToSphere(a+y, mazeTool.width, a, 50),
									Vector3FromCubeToSphere(a+x+y, mazeTool.width, a+x, 50),
									Vector3FromCubeToSphere(a+x+z+y, mazeTool.width, a+x+z, 50),
									Vector3FromCubeToSphere(a+z+y, mazeTool.width, a+z, 50),
									Vector3FromCubeToSphere(a+(0.05f*x)+(0.05f*z)+y, mazeTool.width, a+(0.05f*x)+(0.05f*z), 50),
									Vector3FromCubeToSphere(a+(0.95f*x)+(0.05f*z)+y, mazeTool.width, a+(0.95f*x)+(0.05f*z), 50),
									Vector3FromCubeToSphere(a+(0.95f*x)+(0.95f*z)+y, mazeTool.width, a+(0.95f*x)+(0.95f*z), 50),
									Vector3FromCubeToSphere(a+(0.05f*x)+(0.95f*z)+y, mazeTool.width, a+(0.05f*x)+(0.95f*z), 50),
									Vector3FromCubeToSphere(a+(0.05f*x)+(0.05f*z), mazeTool.width, a+(0.05f*x)+(0.05f*z), 50),
									Vector3FromCubeToSphere(a+(0.95f*x)+(0.05f*z), mazeTool.width, a+(0.95f*x)+(0.05f*z), 50),
									Vector3FromCubeToSphere(a+(0.95f*x)+(0.95f*z), mazeTool.width, a+(0.95f*x)+(0.95f*z), 50),
									Vector3FromCubeToSphere(a+(0.05f*x)+(0.95f*z), mazeTool.width, a+(0.05f*x)+(0.95f*z), 50)
								};
								Vector3 n = Vector3.zero;
								Vector3[] normals = new Vector3[]{n, n, n, n, n, n, n, n, n, n, n, n};
								Vector2 ze = Vector2.zero;
								Vector2[] uvs = new Vector2[]{ze, ze, ze, ze, ze, ze, ze, ze, ze, ze, ze, ze};
								AddMesh(walls, tris, verts, normals, uvs);
								walls.renderer.material = mazeTool.wallMaterial;
							}
						}
					}
				}
			}
		}

		return result;
	}

	private Mesh AddMesh(GameObject obj, int[] tris, Vector3[] verts, Vector3[] normals, Vector2[] uvs) {
		Mesh m = new Mesh();
		m.vertices = verts;
		m.normals = normals;
		m.uv = uvs;
		m.triangles = tris;
		m.Optimize();
		obj.AddComponent<MeshFilter>().mesh = m;
		obj.AddComponent<MeshRenderer>().material = mazeTool.defaultDiffuse;
		return m;
	}
	
	/// <summary>
	/// Returns Pathfinding to the given Point3.
	/// </summary>
	public Pathfinding Pathfind(Point3 pos) {
		return new Pathfinding(data, Point3FromGameToData(new Point3[]{pos}));
	}
	
	/// <summary>
	/// Returns the GameObject that corresponds to the door.
	/// </summary>
	public GameObject GetDoor() {
		return mazeTool.walls[door.x-1, door.y-1].gameObject;
	}
	
	/// <summary>
	/// Returns the game-space location of the door.
	/// </summary>
	public Point3[] FindDoor() {
		return Point3FromDataToGame(door);
	}

	/// <summary>
	/// Returns the game-space location of the key.
	/// </summary>
	public Point3[] FindKey() {
		return Point3FromDataToGame(key);
	}
	
	public Light[,] GetLights(Func<GameObject> getLight) {
		Light[,] result = new Light[mazeTool.width, mazeTool.height];
		GameObject container = new GameObject("lights");
		for (int i=0; i<result.GetLength(0); ++i) {
			for (int j=0; j<result.GetLength(1); ++j) {
				GameObject light = getLight();
				light.name = "light "+(i+1)+" "+(j+1);
				result[i, j] = light.GetComponent<Light>();
				Vector3 v = new Vector3(i+0.5f, 1, j+0.5f);
				v.Scale(mazeTool.transform.localScale);
				light.transform.position = v;
				light.transform.parent = container.transform;
			}
		}
		return result;
	}
	
	/// <summary>
	/// Transforms a Point3 from data-space to game-space.
	/// If the Point3 is representable by a single Point3, it returns the single Point3.
	/// If the Point3 is must be represented by 2 Point3's, it returns an array of 2 Point3's.
	/// This constraint must always be true: (p+1)==output[0]+output[output.Length-1].
	/// </summary>
	public static Point3[] Point3FromDataToGame(Point3 p) {
		Point3 p2 = p;
		if (p.x%2==0) {
			p2.x += 1;
			p.x -= 1;
		}
		if (p.y%2==0) {
			p2.y += 1;
			p.y -= 1;
		}
		if (p==p2)
		return new Point3[]{(p+1)/2};
		else
		return new Point3[]{(p+1)/2, (p2+1)/2};
	}
	
	/// <summary>
	/// Transforms the given Point3(s) from game-space to data-space.
	/// The argument must have (Length==1 || Length==2).
	/// This constraint must always be true: (output+1)==p[0]+p[p.Length-1].
	/// </summary>
	public static Point3 Point3FromGameToData(Point3[] p) {
		return (p[0]+p[p.Length-1]-1);
	}

	public static Vector3 FromDataToCube(Point3 p, int length) {
		return p.ToVector3()/2;
	}

	public static Point3 FromCubeToData(Vector3 v, int length) {
		return new Point3(2*v);
	}

	public static Vector3 Vector3FromCubeToSphere(Vector3 v, int length, Vector3 floor, float radius=-1) {
		//return v;
		// handle radius
		if (radius<0)
			radius = length/2;

		// translate v, center into a cube around Vector3.zero
		Vector3 center = Vector3.one*(length/2);
		v -= center;
		floor -= center;

		// calculate the result
		return floor.normalized*(radius-(v-floor).magnitude);
	}

	public static Vector3 Vector3FromSphereToCube(Vector3 v, int length, float radius=-1) {
		// handle radius
		if (radius<0)
			radius = length/2;

		// reverse engineer v
		float max = Mathf.Max(v.x, v.y, v.z);
		Vector3 floor = v*(0.5f*length/max);
		float avgDistFromCenter = floor.magnitude;

		throw new NotImplementedException("Vector3FromSphereToCube is not yet implemented");
		return v;
	}

	public Vector3 Move(Vector3 from, Vector3 to) {
		return to;
	}

	public MazeCell[,,] MakeCells(GameObject[] cellWalls, GameObject[] cellWallTops, GameObject floor, float radius) {
		return null;
	}
}
