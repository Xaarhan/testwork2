using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;


public class PatchFinderDemo : MonoBehaviour
{


    public static float SCALE = 50;
    // _start is called before the first frame update
    void Start() {

        var n1 = new NodeRect { Rect = new Rect(0, 0, 2, 2) };
        var n2 = new NodeRect { Rect = new Rect(1, 2, 2, 2) };
        var n3 = new NodeRect { Rect = new Rect(3, 1, 1, 2) };
        var n4 = new NodeRect { Rect = new Rect(4, 1, 2, 2) };

        _node_rects = new List<NodeRect> { n1, n2, n3, n4 };
     
        var e1 = new NodeEdge { Node1 = n1, Node2 = n2, P1 = new Vector2(1, 2), P2 = new Vector2(2, 2) };
        var e2 = new NodeEdge { Node1 = n2, Node2 = n3, P1 = new Vector2(3, 2), P2 = new Vector2(3, 3) };
        var e3 = new NodeEdge { Node1 = n3, Node2 = n4, P1 = new Vector2(4, 1), P2 = new Vector2(4, 3) };

        _start = new Vector2(0, 0);
        _end = new Vector2(6, 3);
        _node_edges = new List<NodeEdge> { e1, e2, e3 };

        drawAll();// отрисовка объектов для наглядности

        List<Vector2> patch = CreatePath( _start, _end, _node_edges, 0.4f );

        // draw patch
        UILineRenderer uipatch = Instantiate(def_edge, transform);
        uipatch.Points = new Vector2[patch.Count];
        uipatch.color = Color.black;
        for (int i = 0; i < patch.Count; i++) {
            uipatch.Points[i] = patch[i] * SCALE;
        }

    }


    private List<NodeEdge> _node_edges;
    private List<NodeRect> _node_rects;
    private Vector2 _end;
    private Vector2 _start;



    private List<Vector2> CreatePath(Vector2 start, Vector2 end, List<NodeEdge> edges, float unitRadius) {

        Vector2 curpoint = start;
        List<Vector2> patch = new List<Vector2>();
        patch.Add(curpoint);

        // единообразно выставляем края потрала по отношению к срединной линии , p1 слева, p2 cправа.
        
        
        List<Vector2> middle_points = new List<Vector2>();
        for ( int i = 0; i < edges.Count; i++ ) {
              Vector2 p = edges[i].P1 + (edges[i].P2 - edges[i].P1) / 2.0f;
              if ( checkPoint( curpoint, p, edges[i].P1) < 0 ) {
                   Vector2 ep1 = edges[i].P1;
                   edges[i].P1 = edges[i].P2;
                   edges[i].P2 = ep1;
              }
              middle_points.Add(p);
        }


        Vector2 portal_r = edges[0].P2;
        Vector2 portal_l = edges[0].P1;
        Vector2 portal_mid = middle_points[0];

        Vector2 next_r;
        Vector2 next_l;

        curpoint = start;

        for ( int i = 1; i < edges.Count; i++ ) {
              
              next_l = edges[i].P1; // границы следующего портала
              next_r = edges[i].P2;
             
              if ( checkPoint( curpoint, portal_r, next_r ) >= 0 ) {  // на новом портале возможно сужение правого края воронки
                   if ( checkPoint(curpoint, portal_l, next_r) >= 0) { // новый портал не попал в воронку
                        patch.Add(portal_l);
                        curpoint = portal_l;
                        portal_l = next_l;
                        portal_r = next_r;
                        continue;
                   } else {
                        portal_r = next_r; //  сужаем воронку
                   }
              }

              if ( checkPoint(curpoint, portal_l, next_l) <= 0) {   // на новом портале возможно сужение левого края воронки
                   if ( checkPoint(curpoint, portal_r, next_l) <= 0) {  
                         patch.Add(portal_r);
                         curpoint = portal_r;
                         portal_l = next_l;
                         portal_r = next_r;
                    } else {
                         portal_l = next_l;
                    }
              }
        }

        // проверка попадания в воронку финальной точки
        if (checkPoint(curpoint, portal_r, end) > 0) {  
            if (checkPoint(curpoint, portal_l, end) > 0) {
                patch.Add(portal_l);
            } 
        }

        if (checkPoint(curpoint, portal_l, end) < 0) {   
            if (checkPoint(curpoint, portal_r, end) < 0) {  
                patch.Add(portal_r);
            } 
        }

        patch.Add(end);

        // тут можно было бы еще отступить от краев на ширину агента, если важно то сегодня вечером сделаю

        return patch;
    }





    public bool areCrossing(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4) {
        float v1 = vector_mult(p4.x - p3.x, p4.y - p3.y, p1.x - p3.x, p1.y - p3.y);
        float v2 = vector_mult(p4.x - p3.x, p4.y - p3.y, p2.x - p3.x, p2.y - p3.y);
        float v3 = vector_mult(p2.x - p1.x, p2.y - p1.y, p3.x - p1.x, p3.y - p1.y);
        float v4 = vector_mult(p2.x - p1.x, p2.y - p1.y, p4.x - p1.x, p4.y - p1.y);
        if ((v1 * v2) < 0 && (v3 * v4) < 0) return true;
        return false;
    }



    private float vector_mult(float ax, float ay, float bx, float by) {
        return ax * by - bx * ay;
    }


    private float checkPoint( Vector2 p0, Vector2 p1, Vector2 p2) {
    
        Vector2 a = p1 - p0; // 1
        Vector2 b = p2 - p0; // 2
        float sa = a.x * b.y - b.x * a.y; 
        if (sa > 0.0)
            return 1; // left
        if (sa < 0.0)
            return -1;
        
            return 0;
          
        
    }


    public void drawAll() {


        for ( int i = 0; i < _node_rects.Count; i++ ) {
              NodeRect r = _node_rects[i];
              RectTransform rt = Instantiate(def_rect, transform);
              rt.sizeDelta  = new Vector2( r.Rect.width * SCALE, r.Rect.height * SCALE);
              rt.anchoredPosition = new Vector2(r.Rect.x * SCALE, r.Rect.y * SCALE);
        }

        for ( int i = 0; i < _node_edges.Count; i++ ) {
              UILineRenderer line = Instantiate(def_edge, transform);
              line.Points[0] = _node_edges[i].P1 * SCALE;
              line.Points[1] = _node_edges[i].P2 * SCALE;
        }

        UILineRenderer middle_line = Instantiate(def_edge, transform);
        middle_line.color = Color.white;
        middle_line.Points = new Vector2[_node_edges.Count + 2];
        middle_line.Points[0] = _start * SCALE;
        for (int i = 0; i < _node_edges.Count; i++) {
            Vector2 p = _node_edges[i].P1 + (_node_edges[i].P2 - _node_edges[i].P1) / 2.0f;
            middle_line.Points[i + 1] = p * SCALE;
            
        }
        middle_line.Points[_node_edges.Count + 1] = _end * SCALE;

    }


    public RectTransform def_rect;
    public UILineRenderer def_edge;


}
