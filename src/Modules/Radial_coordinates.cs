using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace Radial_coordinates
{
    /// <summary>
    /// This module computes coordinates for the nodes of the tree in a "radial" style. The root node of the tree is placed at the
    /// center of the tree, and branches expand from it in a way that makes sure they do not intersect with each other.
    /// 
    /// For the default value of the parameters below, let $n$ be the number of taxa (i.e. leaves) in the tree.
    /// </summary>
    /// 
    /// <description>
    /// <![CDATA[
    /// ## Further information
    /// 
    /// This code is based on the algorithm used by [FigTree](http://tree.bio.ed.ac.uk/software/figtree/), which is available under
    /// a GPLv2 licence [here](https://github.com/rambaut/figtree/blob/9f0e0ae495bdeaa344f7eaecf3082af3b2588097/src/figtree/treeviewer/treelayouts/RadialTreeLayout.java).
    /// 
    /// Here is an example of a tree drawn using radial coordinates (and with the appropriate shape for the _Branches_):
    /// 
    /// <p align="center">
    /// <img height="800" src="data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4NCjxzdmcgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiIHZpZXdCb3g9IjAgMCAzMDQgMzA0IiB2ZXJzaW9uPSIxLjEiIHN0eWxlPSJmb250LXN5bnRoZXNpczogbm9uZTsiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+DQogIDxwYXRoIGQ9Ik0gMCAwIEwgMzA0IDAgTCAzMDQgMzA0IEwgMCAzMDQgWiAiIHN0cm9rZT0ibm9uZSIgZmlsbD0iI0ZGRkZGRiIgZmlsbC1vcGFjaXR5PSIxIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDAsMCkiIC8+DQogIDxwYXRoIGQ9Ik0gMTM4LjIzMDYxNTgyNDA2NDE2IDE0MC40MDI5ODI1NjI5NzczNCBMIDEyOS4xOTMyNDY3ODE2ODIyMyAxNjQuMTg3NzU2NTgwNDg3MTMgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSwxMiwxMikiIGlkPSIzNDE5MzkxOC1mZGYzLTQwMTItYTg5Ni02YTE1MGIzZmZlOTQiIC8+DQogIDxwYXRoIGQ9Ik0gMTI5LjE5MzI0Njc4MTY4MjIzIDE2NC4xODc3NTY1ODA0ODcxMyBMIDE5OC43Mjg2NTE1ODQ0NzI0NiAxODMuNTA4MTEwNDUyMzMxMTIgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSwxMiwxMikiIGlkPSIyNDk4MGM5Yi00NDZiLTQ1YWEtOWM1OC1jN2IxNGY2MjAzZjMiIC8+DQogIDxwYXRoIGQ9Ik0gMTk4LjcyODY1MTU4NDQ3MjQ2IDE4My41MDgxMTA0NTIzMzExMiBMIDI3MC45NDIzNTAxNDU1NDIgMTkzLjI4ODcwMjcwODg5Mzc0ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsMTIsMTIpIiBpZD0iNzNhYWU3MzUtMDgzMy00MWNhLTg1NmYtNGJhNTgyMDkxMmVmIiAvPg0KICA8cGF0aCBkPSJNIDE5OC43Mjg2NTE1ODQ0NzI0NiAxODMuNTA4MTEwNDUyMzMxMTIgTCAyNjMuODczNTcwMTg3OTYxMSAyMTEuODkyNDk0NzA2NTUzNCAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDEyLDEyKSIgaWQ9ImU3NzI0YTg1LWI0NzUtNDFlMy1iNmE2LTRhNjkyYWU5ZTg3NCIgLz4NCiAgPHBhdGggZD0iTSAxMjkuMTkzMjQ2NzgxNjgyMjMgMTY0LjE4Nzc1NjU4MDQ4NzEzIEwgMTEyLjAwMzE0OTM0NTgzOTQyIDE4NC40MjAyOTM4MjQ5NTI0ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsMTIsMTIpIiBpZD0iM2EwNjg2YzktMmQxYy00NzgzLWJiYWItMDQ5ZGE4YWI0Zjc0IiAvPg0KICA8cGF0aCBkPSJNIDExMi4wMDMxNDkzNDU4Mzk0MiAxODQuNDIwMjkzODI0OTUyNCBMIDEyOS43MDYwNzk0NDM4MjM2OCAyMTQuMTMwOTU1NTQ3NDIyNiAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDEyLDEyKSIgaWQ9IjFkYThmNjA4LWIzOTktNGExNy1iYmViLTc4MWY3Mzc0OWU5NSIgLz4NCiAgPHBhdGggZD0iTSAxMjkuNzA2MDc5NDQzODIzNjggMjE0LjEzMDk1NTU0NzQyMjYgTCAxNTIuNjI2MjA5MzU4MjgwODIgMjQxLjEwNzY3MTg3MzM3NjM1ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsMTIsMTIpIiBpZD0iZTM2ZWFkMGQtZTU1ZS00ZTVkLWFjNWItNGQ4NGIyNWQ0NDYyIiAvPg0KICA8cGF0aCBkPSJNIDE1Mi42MjYyMDkzNTgyODA4MiAyNDEuMTA3NjcxODczMzc2MzUgTCAxODAuMTk5MTY5NDIzMTY2NyAyNjQuNjg2MTg2NTE2MjgwMTUgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSwxMiwxMikiIGlkPSIzMzFiZGY0ZS02OWU4LTQ5YzUtYjgyNC02OTA0MDcxYWViM2QiIC8+DQogIDxwYXRoIGQ9Ik0gMTUyLjYyNjIwOTM1ODI4MDgyIDI0MS4xMDc2NzE4NzMzNzYzNSBMIDE3MC4zMjkxMzk0NTYyNjUwNyAyNzAuODE4MzMzNTk1ODQ2NSAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDEyLDEyKSIgaWQ9IjE3Y2Y1N2I2LWEzYzMtNGFjZC05OTEwLWRkOGQyZWQyODJmZCIgLz4NCiAgPHBhdGggZD0iTSAxMjkuNzA2MDc5NDQzODIzNjggMjE0LjEzMDk1NTU0NzQyMjYgTCAxNDEuOTA2MTA3NDIyODc0MTMgMjgwICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsMTIsMTIpIiBpZD0iMWJhYWNjOTAtNWY2Zi00NWI0LWEzNTEtOThlNzJjNjY5ZjhiIiAvPg0KICA8cGF0aCBkPSJNIDExMi4wMDMxNDkzNDU4Mzk0MiAxODQuNDIwMjkzODI0OTUyNCBMIDg1Ljk0NTE4MTkwNDQ0Mzk0IDE5NS43NzQwNDc1MjY2NDEzICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsMTIsMTIpIiBpZD0iMTkyMjdjOGYtYjE4Ny00ODEyLTk1MWUtYmZmMDgwZjQyYzNjIiAvPg0KICA8cGF0aCBkPSJNIDg1Ljk0NTE4MTkwNDQ0Mzk0IDE5NS43NzQwNDc1MjY2NDEzIEwgNzIuNjY3OTg0MzMwOTU1NzMgMjE4LjA1NzA0MzgxODQ5Mzk4ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsMTIsMTIpIiBpZD0iNDQ4ZmNlNjEtMzJlOC00YWM0LWIyNDUtNjE1Zjk3NDg1NjU3IiAvPg0KICA8cGF0aCBkPSJNIDcyLjY2Nzk4NDMzMDk1NTczIDIxOC4wNTcwNDM4MTg0OTM5OCBMIDYzLjUxNzk2MzM0NjY2NzkyNiAyNjcuNDU4ODI3MTU3OTI3MDMgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSwxMiwxMikiIGlkPSJhZTViNTQzMC03ODY4LTQzNmItOGM4OC05Njc4ZGMyNDYwMGYiIC8+DQogIDxwYXRoIGQ9Ik0gNzIuNjY3OTg0MzMwOTU1NzMgMjE4LjA1NzA0MzgxODQ5Mzk4IEwgNTUuNDc3ODg2ODk1MTEyOTIgMjM4LjI4OTU4MTA2Mjk1OTI2ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsMTIsMTIpIiBpZD0iNzM1NzBmZjktN2QwOC00OWEwLTljZmYtMGZiODUyMzU1MGUyIiAvPg0KICA8cGF0aCBkPSJNIDU1LjQ3Nzg4Njg5NTExMjkyIDIzOC4yODk1ODEwNjI5NTkyNiBMIDQyLjIwMDY4OTMyMTYyNDcgMjYwLjU3MjU3NzM1NDgxMTg3ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsMTIsMTIpIiBpZD0iOTJjOGUzNWYtYWYwNC00NTQyLTljMjEtZWQxYzM3ZGMxZGNmIiAvPg0KICA8cGF0aCBkPSJNIDU1LjQ3Nzg4Njg5NTExMjkyIDIzOC4yODk1ODEwNjI5NTkyNiBMIDM0Ljc5ODE2Njg0NjQ0ODQ4IDI1NS45NzM0NjcwNDUxMzcwOCAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDEyLDEyKSIgaWQ9ImVlMjg2OGViLTI0NTgtNGI5MC05MjViLTJkNDA4NTJhZjAyMyIgLz4NCiAgPHBhdGggZD0iTSA4NS45NDUxODE5MDQ0NDM5NCAxOTUuNzc0MDQ3NTI2NjQxMyBMIDU2LjY5OTY0MTM0NTU0MzkgMTk1Ljc3NDA0NzUyNjY0MTMgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSwxMiwxMikiIGlkPSJjNTk1YWNhZS03MzMyLTQ3NDYtOGYwNS1kNDk1YTEwNzA0ZTMiIC8+DQogIDxwYXRoIGQ9Ik0gNTYuNjk5NjQxMzQ1NTQzOSAxOTUuNzc0MDQ3NTI2NjQxMyBMIDI4Ljg4NTQ3OTQyNDQyNzggMjAzLjUwMjE4OTA3NTM3ODk0ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsMTIsMTIpIiBpZD0iZDA2M2FjNjQtYTU0My00ZjFlLThmZjItOGQ1ZDYwMmUwNWFhIiAvPg0KICA8cGF0aCBkPSJNIDI4Ljg4NTQ3OTQyNDQyNzggMjAzLjUwMjE4OTA3NTM3ODk0IEwgMi44Mjc1MTE5ODMwMzIzMiAyMTQuODU1OTQyNzc3MDY3ODQgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSwxMiwxMikiIGlkPSJmZTM1NGE2Yi1jNDhhLTRlZDgtYjRmYi1hM2I4NWFlNjJkZjciIC8+DQogIDxwYXRoIGQ9Ik0gMjguODg1NDc5NDI0NDI3OCAyMDMuNTAyMTg5MDc1Mzc4OTQgTCAyLjU5NzUyNTc5OTY4Nzk3NjRFLTE0IDIwNy40MTQ0MjU5NzgwMDQgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSwxMiwxMikiIGlkPSIwZDhhZmI5Mi02ZWQzLTQyY2EtYTA4Yy0xOWRlMGEzYTliOTMiIC8+DQogIDxwYXRoIGQ9Ik0gNTYuNjk5NjQxMzQ1NTQzOSAxOTUuNzc0MDQ3NTI2NjQxMyBMIDI4Ljg4NTQ3OTQyNDQyNzggMTg4LjA0NTkwNTk3NzkwMzcxICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsMTIsMTIpIiBpZD0iMDY5MTVkYjItNWE2Ni00MWE3LWFmMzctNThiODM2YWFlZDIyIiAvPg0KICA8cGF0aCBkPSJNIDI4Ljg4NTQ3OTQyNDQyNzggMTg4LjA0NTkwNTk3NzkwMzcxIEwgMCAxODQuMTMzNjY5MDc1Mjc4NjggIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSwxMiwxMikiIGlkPSJiNGZjOTdiZC1kNjY2LTQ3M2EtYjc1Yi1iOGVmZWU2YjZjODQiIC8+DQogIDxwYXRoIGQ9Ik0gMjguODg1NDc5NDI0NDI3OCAxODguMDQ1OTA1OTc3OTAzNzEgTCAyLjgyNzUxMTk4MzAzMjMyIDE3Ni42OTIxNTIyNzYyMTQ4NCAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDEyLDEyKSIgaWQ9IjQ5NmQ2YzUzLTRhNTctNDEwMC1hMWU5LTBiZmI4MWYzNmNkYiIgLz4NCiAgPHBhdGggZD0iTSAxMzguMjMwNjE1ODI0MDY0MTYgMTQwLjQwMjk4MjU2Mjk3NzM0IEwgMTQ3LjI2Nzk4NDg2NjQ0NjA2IDExNi42MTgyMDg1NDU0Njc1NCAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDEyLDEyKSIgaWQ9IjNjNDcyYzYxLWRkOGQtNGI2YS1hZDA4LTAyMzFjYTUxZTQ1OSIgLz4NCiAgPHBhdGggZD0iTSAxNDcuMjY3OTg0ODY2NDQ2MDYgMTE2LjYxODIwODU0NTQ2NzU0IEwgMTA0LjI5Mjc0MTI3NjgzOSA2Ni4wMzY4NjU0MzQzMDQzMiAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDEyLDEyKSIgaWQ9IjYyOGFhNmQzLTg5YjEtNGQ1My05ZDNkLTlmNmU0MGY0ZjU5NSIgLz4NCiAgPHBhdGggZD0iTSAxMDQuMjkyNzQxMjc2ODM5IDY2LjAzNjg2NTQzNDMwNDMyIEwgNTIuNTkzNDQxMTU1MTc3ODggMjEuODI3MTUwNDc4ODU5NjY4ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsMTIsMTIpIiBpZD0iZGNkMTEzNjEtOWMxYS00OGI3LTk1ZDItZWI2N2RhZmI1MjYyIiAvPg0KICA8cGF0aCBkPSJNIDEwNC4yOTI3NDEyNzY4MzkgNjYuMDM2ODY1NDM0MzA0MzIgTCA3MS4wOTk3NDczNDMxMTg0MiAxMC4zMjkzNzQ3MDQ2NzI3MTQgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSwxMiwxMikiIGlkPSIzZjViMWIzNC0xMzJhLTRjYjMtOGY2NS0xYmY5MmJhMzRlNGUiIC8+DQogIDxwYXRoIGQ9Ik0gMTQ3LjI2Nzk4NDg2NjQ0NjA2IDExNi42MTgyMDg1NDU0Njc1NCBMIDE2NC40NTgwODIzMDIyODg4NiA5Ni4zODU2NzEzMDEwMDIyNSAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDEyLDEyKSIgaWQ9IjUwZWM5NzQzLTBhYWMtNDI2Yy1hNDk0LWNkNmNmZTYzZDExYSIgLz4NCiAgPHBhdGggZD0iTSAxNjQuNDU4MDgyMzAyMjg4ODYgOTYuMzg1NjcxMzAxMDAyMjUgTCAxNzcuNzM1Mjc5ODc1Nzc3MDUgNzQuMTAyNjc1MDA5MTQ5NTkgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSwxMiwxMikiIGlkPSI5OGU2Zjg2MS1jOWFiLTQ4NjctODBhMC0zOWMyZjUyYmYwNjMiIC8+DQogIDxwYXRoIGQ9Ik0gMTc3LjczNTI3OTg3NTc3NzA1IDc0LjEwMjY3NTAwOTE0OTU5IEwgMTY0LjAxMDI0ODM5OTM0NTMzIDAgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSwxMiwxMikiIGlkPSI2ZjAzZjYyMS1mYmE5LTQ3MmItYTc4Yi1lZGNiYzQwNzM0ZjgiIC8+DQogIDxwYXRoIGQ9Ik0gMTc3LjczNTI3OTg3NTc3NzA1IDc0LjEwMjY3NTAwOTE0OTU5IEwgMTk0LjkyNTM3NzMxMTYxOTkgNTMuODcwMTM3NzY0Njg0MzEgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSwxMiwxMikiIGlkPSJhMWM3ZGEwNy0zMzRkLTRkYjAtYjg4Yi0yYjZiZmNjNzI2NjciIC8+DQogIDxwYXRoIGQ9Ik0gMTk0LjkyNTM3NzMxMTYxOTkgNTMuODcwMTM3NzY0Njg0MzEgTCAyMDMuOTYyNzQ2MzU0MDAxNzggMzAuMDg1MzYzNzQ3MTc0NTE1ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsMTIsMTIpIiBpZD0iOTljNWRhZWMtNzk4NC00Y2UzLTkwMzAtN2M4Y2Q0Y2EwMmM5IiAvPg0KICA8cGF0aCBkPSJNIDIwMy45NjI3NDYzNTQwMDE3OCAzMC4wODUzNjM3NDcxNzQ1MTUgTCAyMDguNTM3NzU2ODQ2MTQ1NyA1LjM4NDQ3MjA3NzQ1Nzk2NyAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDEyLDEyKSIgaWQ9IjNmZjk0YzQ2LTQyNzgtNDYwYy05ODI3LTAzMjVjMzZhMTBjNyIgLz4NCiAgPHBhdGggZD0iTSAyMDMuOTYyNzQ2MzU0MDAxNzggMzAuMDg1MzYzNzQ3MTc0NTE1IEwgMjE3LjIzOTk0MzkyNzQ5IDcuODAyMzY3NDU1MzIxODY3ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsMTIsMTIpIiBpZD0iMDkyYjkwYmEtZWMwMS00YjE1LTg2MTUtMDk3NGJmNTRlY2ZkIiAvPg0KICA8cGF0aCBkPSJNIDE5NC45MjUzNzczMTE2MTk5IDUzLjg3MDEzNzc2NDY4NDMxIEwgMjE4LjU4NTUxNjYzMzQ1MTgyIDM5LjE3MDMzOTAwNzEyNzg2NSAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDEyLDEyKSIgaWQ9ImE5MDY4MTIxLTI3MjEtNGFlNS1iMTQzLTQyMzUxOWNhZjMzYyIgLz4NCiAgPHBhdGggZD0iTSAyMTguNTg1NTE2NjMzNDUxODIgMzkuMTcwMzM5MDA3MTI3ODY1IEwgMjM5LjI2NTIzNjY4MjExNjI3IDIxLjQ4NjQ1MzAyNDk0OTk5NSAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDEyLDEyKSIgaWQ9IjNhMjhhMTgyLTU5ZjEtNDFlNi04OTJjLTcxMmYxMzI1MzQ4NCIgLz4NCiAgPHBhdGggZD0iTSAyMTguNTg1NTE2NjMzNDUxODIgMzkuMTcwMzM5MDA3MTI3ODY1IEwgMjQ0LjY0MzQ4NDA3NDg0NzMgMjcuODE2NTg1MzA1NDM4OTYgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSwxMiwxMikiIGlkPSJkYTNmNTZmOC0zYTM4LTRiZjQtYjEzYi1iODFjYzQ5NDM4ODgiIC8+DQogIDxwYXRoIGQ9Ik0gMTY0LjQ1ODA4MjMwMjI4ODg2IDk2LjM4NTY3MTMwMTAwMjI1IEwgMjgwIDgwLjczNjcyMzY5MDUwMiAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDEyLDEyKSIgaWQ9IjczZTY2YTlmLTkzYzAtNDg5Yy04M2FmLTI1MTM1NjM0NTliMyIgLz4NCjwvc3ZnPg==" />
    /// </p>
    /// 
    /// ]]>
    /// </description>

    public static class MyModule
    {
        public const string Name = "Radial";
        public const string HelpText = "Computes the coordinates for a radial tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const string Id = "95b61284-b870-48b9-b51c-3276f7d89df1";
        public const ModuleTypes ModuleType = ModuleTypes.Coordinate;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            double defaultHeight = Math.Min(10000, tree.GetLeaves().Count * 14);

            return new List<(string, string)>()
            {
                ( "Tree size", "Group:2" ),
                
                /// <param name="Width:" default="14 $\cdot n$">
                /// This parameter determines the width of the area covered by the tree.
                /// </param>
                ( "Width:", "NumericUpDown:" + defaultHeight.ToString(0) + "[\"0\",\"Infinity\"]" ),
                
                /// <param name="Height:" default="14 $\cdot n$">
                /// This parameter determines the height of the area covered by the tree.
                /// </param>
                ( "Height:", "NumericUpDown:" + defaultHeight.ToString(0) + "[\"0\",\"Infinity\"]" ),

                ( "Tree area", "Group:2" ),
                
                /// <param name="Start angle:">
                /// This parameter determines the angle for the first split in the tree. Changing it has the effect of rotating
                /// the tree.
                /// </param>
                ( "Start angle:", "Slider:0[\"0\",\"360\",\"0°\"]" ),
                
                /// <param name="Sweep angle:">
                /// This parameter determines the angular size of the tree.
                /// </param>
                ( "Sweep angle:", "Slider:360[\"1\",\"360\",\"0°\"]" ),
                
                /// <param name="Apply">
                /// This button applies changes to the other parameter values and signals that the tree needs to be redrawn.
                /// </param>
                ( "Apply", "Button:" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>()
                {
                    { "Tree size", ControlStatus.Enabled },
                    { "Width:", ControlStatus.Enabled },
                    { "Height:", ControlStatus.Enabled },
                    { "Apply", ControlStatus.Enabled }
                };

            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };

            return (bool)currentParameterValues["Apply"];
        }

        public static Dictionary<string, Point> GetCoordinates(TreeNode tree, Dictionary<string, object> parameterValues)
        {
            List<TreeNode> nodes = tree.GetChildrenRecursive();

            Dictionary<string, Point> storedPos = new Dictionary<string, Point>();

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            void setNodePosition(TreeNode node, double angleStart, double angleFinish, double xPosition, double yPosition)
            {
                //Adapted from: https://github.com/rambaut/figtree/blob/9f0e0ae495bdeaa344f7eaecf3082af3b2588097/src/figtree/treeviewer/treelayouts/RadialTreeLayout.java

                double branchAngle = (angleStart + angleFinish) / 2.0;

                double directionX = Math.Cos(branchAngle);
                double directionY = Math.Sin(branchAngle);
                double length = (node.Parent != null) ? !double.IsNaN(node.Length) ? node.Length : 1 : 0;
                Point nodePoint = new Point(xPosition + (length * directionX), yPosition + (length * directionY));

                minX = Math.Min(minX, nodePoint.X);
                maxX = Math.Max(maxX, nodePoint.X);
                minY = Math.Min(minY, nodePoint.Y);
                maxY = Math.Max(maxY, nodePoint.Y);

                if (node.Children.Count > 0)
                {
                    List<TreeNode> children = node.Children;
                    List<int> leafCounts = new List<int>();
                    int sumLeafCount = 0;

                    for (int i = 0; i < children.Count; i++)
                    {
                        leafCounts.Add(children[i].GetLeaves().Count);
                        sumLeafCount += leafCounts[i];
                    }

                    double span = (angleFinish - angleStart);

                    if (node.Parent != null)
                    {
                        angleStart = branchAngle - (span / 2.0);
                        angleFinish = branchAngle + (span / 2.0);
                    }

                    double a2 = angleStart;

                    bool rotate = false;

                    for (int i = 0; i < children.Count; i++)
                    {
                        int index = i;

                        if (rotate)
                        {
                            index = children.Count - i - 1;
                        }

                        TreeNode child = children[index];

                        double childLength = (child.Parent != null) ? !double.IsNaN(child.Length) ? child.Length : 1 : 0;

                        double a1 = a2;

                        a2 = a1 + (span * leafCounts[index] / sumLeafCount);

                        setNodePosition(child, a1, a2, nodePoint.X, nodePoint.Y);
                    }
                }

                storedPos.Add(node.Id, nodePoint);
            }

            double startAngle = (double)parameterValues["Start angle:"] * Math.PI / 180;
            double sweepAngle = (double)parameterValues["Sweep angle:"] * Math.PI / 180;

            setNodePosition(tree, startAngle, startAngle + sweepAngle, 0, 0);

            double width = (double)parameterValues["Width:"];
            double height = (double)parameterValues["Height:"];

            List<string> nodeIds = new List<string>(from el in storedPos select el.Key);

            double hFactor = maxX != minX ? ((maxX - minX) / width) : 1;
            double vFactor = maxY != minY ? ((maxY - minY) / height) : 1;

            for (int i = 0; i < nodeIds.Count; i++)
            {
                storedPos[nodeIds[i]] = new Point((storedPos[nodeIds[i]].X - minX) / hFactor, (storedPos[nodeIds[i]].Y - minY) / vFactor);
            }

            static double dist(Point p1, Point p2)
            {
                return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
            };

            Point rootPoint = storedPos[tree.Id];

            double scaleFactor = 0;
            int factorCount = 0;

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Parent != null && !double.IsNaN(nodes[i].Length) && nodes[i].Length > 0)
                {
                    if (!double.IsNaN(nodes[i].Length) && nodes[i].Length != 0)
                    {
                        scaleFactor += dist(storedPos[nodes[i].Id], storedPos[nodes[i].Parent.Id]) / nodes[i].Length;
                    }
                    else
                    {
                        scaleFactor += dist(storedPos[nodes[i].Id], storedPos[nodes[i].Parent.Id]);
                    }

                    factorCount++;
                }
            }

            scaleFactor /= factorCount;

            if (double.IsNaN(scaleFactor))
            {
                scaleFactor = 1;
            }

            double longestDownStream = tree.LongestDownstreamLength();

            if (double.IsNaN(longestDownStream))
            {
                longestDownStream = 1;
            }

            double rootLength = scaleFactor * (!double.IsNaN(tree.Length) ? tree.Length : (longestDownStream * 0.2));
            List<double> thetas = new List<double>();

            for (int i = 0; i < tree.Children.Count; i++)
            {
                Point childPoint = storedPos[tree.Children[i].Id];
                thetas.Add(Math.Atan2(childPoint.Y - rootPoint.Y, childPoint.X - rootPoint.X));
            }

            double theta = thetas.Average();

            if (Math.Abs(Math.Cos(theta)) >= 0.7071)
            {
                int counts = 0;
                for (int i = 0; i < tree.Children.Count; i++)
                {
                    Point childPoint = storedPos[tree.Children[i].Id];
                    counts += Math.Sign(childPoint.X - rootPoint.X);
                }

                if (counts * Math.Cos(theta) > 0)
                {
                    theta += Math.PI;
                }

                storedPos[Modules.RootNodeId] = new Point(rootPoint.X + Math.Cos(theta) * rootLength, rootPoint.Y + Math.Sin(theta) * rootLength);
            }
            else
            {
                int counts = 0;
                for (int i = 0; i < tree.Children.Count; i++)
                {
                    Point childPoint = storedPos[tree.Children[i].Id];
                    counts += Math.Sign(childPoint.Y - rootPoint.Y);
                }

                if (counts * Math.Sin(theta) > 0)
                {
                    theta += Math.PI;
                }

                storedPos[Modules.RootNodeId] = new Point(rootPoint.X + Math.Cos(theta) * rootLength, rootPoint.Y + Math.Sin(theta) * rootLength);
            }

            storedPos[Id] = new Point(scaleFactor, 0);

            return storedPos;
        }
    }
}