/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace RectangularCoordinates
{
    /// <summary>
    /// This module computes coordinates for the nodes of the tree in a "rectangular" style. The root node of the tree is placed at the
    /// left, and branches expand horizontally towards the right (the orientation of the tree can be changed with the [Rotation](#rotation)
    /// parameter.
    /// 
    /// For the default value of the parameters below, let $n$ be the number of taxa (i.e. leaves) in the tree.
    /// </summary>
    /// 
    /// <description>
    /// <![CDATA[
    /// ## Further information
    /// 
    /// Here is an example of a tree drawn using rectangular coordinates (and with the appropriate shape for the _Branches_):
    /// 
    /// <p align="center">
    /// <img width="800" src="data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4NCjxzdmcgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiIHZpZXdCb3g9IjAgMCAzNTQgMjkwIiB2ZXJzaW9uPSIxLjEiIHN0eWxlPSJmb250LXN5bnRoZXNpczogbm9uZTsiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+DQogIDxwYXRoIGQ9Ik0gMCAwIEwgMzU0IDAgTCAzNTQgMjkwIEwgMCAyOTAgWiAiIHN0cm9rZT0ibm9uZSIgZmlsbD0iI0ZGRkZGRiIgZmlsbC1vcGFjaXR5PSIxIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDAsMCkiIC8+DQogIDxwYXRoIGQ9Ik0gLTMwIDEzMS40Njg3NSBMIDAgMTMxLjQ2ODc1ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsNDIsNSkiIGlkPSI2NjVkMTVjOC04OWIxLTQyMmItYTBmZC0zYTE5Y2MwZDI3MWMiIC8+DQogIDxwYXRoIGQ9Ik0gMCAxMzEuNDY4NzUgTCAwIDQ4LjU2MjUgTCA3NS4yNDI1MjQ1NTY3MTAwMSA0OC41NjI1ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsNDIsNSkiIGlkPSIzNDE5MzkxOC1mZGYzLTQwMTItYTg5Ni02YTE1MGIzZmZlOTQiIC8+DQogIDxwYXRoIGQ9Ik0gNzUuMjQyNTI0NTU2NzEwMDEgNDguNTYyNSBMIDc1LjI0MjUyNDU1NjcxMDAxIDE0IEwgMTM0LjYxOTUyOTgyNTg5MTAyIDE0ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsNDIsNSkiIGlkPSIyNDk4MGM5Yi00NDZiLTQ1YWEtOWM1OC1jN2IxNGY2MjAzZjMiIC8+DQogIDxwYXRoIGQ9Ik0gMTM0LjYxOTUyOTgyNTg5MTAyIDE0IEwgMTM0LjYxOTUyOTgyNTg5MTAyIDcgTCAxNTguMzE0MTQ1MTkwODAzMDYgNyAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iNzNhYWU3MzUtMDgzMy00MWNhLTg1NmYtNGJhNTgyMDkxMmVmIiAvPg0KICA8cGF0aCBkPSJNIDEzNC42MTk1Mjk4MjU4OTEwMiAxNCBMIDEzNC42MTk1Mjk4MjU4OTEwMiAyMSBMIDEzNS4zMjQyMTM3NTcyODIyMiAyMSAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iZTc3MjRhODUtYjQ3NS00MWUzLWI2YTYtNGE2OTJhZTllODc0IiAvPg0KICA8cGF0aCBkPSJNIDc1LjI0MjUyNDU1NjcxMDAxIDQ4LjU2MjUgTCA3NS4yNDI1MjQ1NTY3MTAwMSA4My4xMjUgTCAxMTEuNDA5NTMyNTkzNjAxMzYgODMuMTI1ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsNDIsNSkiIGlkPSIzYTA2ODZjOS0yZDFjLTQ3ODMtYmJhYi0wNDlkYThhYjRmNzQiIC8+DQogIDxwYXRoIGQ9Ik0gMTExLjQwOTUzMjU5MzYwMTM2IDgzLjEyNSBMIDExMS40MDk1MzI1OTM2MDEzNiA1Mi41IEwgMTQyLjYxMzI5OTY5NjUwNTIgNTIuNSAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iMWRhOGY2MDgtYjM5OS00YTE3LWJiZWItNzgxZjczNzQ5ZTk1IiAvPg0KICA8cGF0aCBkPSJNIDE0Mi42MTMyOTk2OTY1MDUyIDUyLjUgTCAxNDIuNjEzMjk5Njk2NTA1MiA0MiBMIDE3NC43NjYzODY2Nzc5ODQxNyA0MiAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iZTM2ZWFkMGQtZTU1ZS00ZTVkLWFjNWItNGQ4NGIyNWQ0NDYyIiAvPg0KICA8cGF0aCBkPSJNIDE3NC43NjYzODY2Nzc5ODQxNyA0MiBMIDE3NC43NjYzODY2Nzc5ODQxNyAzNSBMIDIwOS45NzM5MTA5NDA4MTcwMyAzNSAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iMzMxYmRmNGUtNjllOC00OWM1LWI4MjQtNjkwNDA3MWFlYjNkIiAvPg0KICA8cGF0aCBkPSJNIDE3NC43NjYzODY2Nzc5ODQxNyA0MiBMIDE3NC43NjYzODY2Nzc5ODQxNyA0OSBMIDE4MC40NjIwNTM1NjM5MjA2OCA0OSAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iMTdjZjU3YjYtYTNjMy00YWNkLTk5MTAtZGQ4ZDJlZDI4MmZkIiAvPg0KICA8cGF0aCBkPSJNIDE0Mi42MTMyOTk2OTY1MDUyIDUyLjUgTCAxNDIuNjEzMjk5Njk2NTA1MiA2MyBMIDE1NC43MTcyNzk4ODU1OTI4IDYzICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsNDIsNSkiIGlkPSIxYmFhY2M5MC01ZjZmLTQ1YjQtYTM1MS05OGU3MmM2NjlmOGIiIC8+DQogIDxwYXRoIGQ9Ik0gMTExLjQwOTUzMjU5MzYwMTM2IDgzLjEyNSBMIDExMS40MDk1MzI1OTM2MDEzNiAxMTMuNzUgTCAxMzMuMDU5NjM0MTIyNDk4MzggMTEzLjc1ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsNDIsNSkiIGlkPSIxOTIyN2M4Zi1iMTg3LTQ4MTItOTUxZS1iZmYwODBmNDJjM2MiIC8+DQogIDxwYXRoIGQ9Ik0gMTMzLjA1OTYzNDEyMjQ5ODM4IDExMy43NSBMIDEzMy4wNTk2MzQxMjI0OTgzOCA4Ny41IEwgMTUxLjY0MDM3ODE4MDQxMjI4IDg3LjUgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSw0Miw1KSIgaWQ9IjQ0OGZjZTYxLTMyZTgtNGFjNC1iMjQ1LTYxNWY5NzQ4NTY1NyIgLz4NCiAgPHBhdGggZD0iTSAxNTEuNjQwMzc4MTgwNDEyMjggODcuNSBMIDE1MS42NDAzNzgxODA0MTIyOCA3NyBMIDE3MC41NjAzMDg1NjAyNjk3MyA3NyAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iYWU1YjU0MzAtNzg2OC00MzZiLThjODgtOTY3OGRjMjQ2MDBmIiAvPg0KICA8cGF0aCBkPSJNIDE1MS42NDAzNzgxODA0MTIyOCA4Ny41IEwgMTUxLjY0MDM3ODE4MDQxMjI4IDk4IEwgMTYxLjM3OTU0Mzk0MTc2MjY1IDk4ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsNDIsNSkiIGlkPSI3MzU3MGZmOS03ZDA4LTQ5YTAtOWNmZi0wZmI4NTIzNTUwZTIiIC8+DQogIDxwYXRoIGQ9Ik0gMTYxLjM3OTU0Mzk0MTc2MjY1IDk4IEwgMTYxLjM3OTU0Mzk0MTc2MjY1IDkxIEwgMTYxLjkyODQ0MTMzMDMzNzYgOTEgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSw0Miw1KSIgaWQ9IjkyYzhlMzVmLWFmMDQtNDU0Mi05YzIxLWVkMWMzN2RjMWRjZiIgLz4NCiAgPHBhdGggZD0iTSAxNjEuMzc5NTQzOTQxNzYyNjUgOTggTCAxNjEuMzc5NTQzOTQxNzYyNjUgMTA1IEwgMTc4LjEyOTkzODk0OTA4NTIxIDEwNSAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iZWUyODY4ZWItMjQ1OC00YjkwLTkyNWItMmQ0MDg1MmFmMDIzIiAvPg0KICA8cGF0aCBkPSJNIDEzMy4wNTk2MzQxMjI0OTgzOCAxMTMuNzUgTCAxMzMuMDU5NjM0MTIyNDk4MzggMTQwIEwgMTcxLjY5NDk3MTgxMDIyMDUgMTQwICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsNDIsNSkiIGlkPSJjNTk1YWNhZS03MzMyLTQ3NDYtOGYwNS1kNDk1YTEwNzA0ZTMiIC8+DQogIDxwYXRoIGQ9Ik0gMTcxLjY5NDk3MTgxMDIyMDUgMTQwIEwgMTcxLjY5NDk3MTgxMDIyMDUgMTI1Ljk5OTk5OTk5OTk5OTk5IEwgMjUxLjk2NDM4MTYxNzQ5MzM2IDEyNS45OTk5OTk5OTk5OTk5OSAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iZDA2M2FjNjQtYTU0My00ZjFlLThmZjItOGQ1ZDYwMmUwNWFhIiAvPg0KICA8cGF0aCBkPSJNIDI1MS45NjQzODE2MTc0OTMzNiAxMjUuOTk5OTk5OTk5OTk5OTkgTCAyNTEuOTY0MzgxNjE3NDkzMzYgMTE5IEwgMjkwLjI1OTI2ODgyODQxMDQgMTE5ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsNDIsNSkiIGlkPSJmZTM1NGE2Yi1jNDhhLTRlZDgtYjRmYi1hM2I4NWFlNjJkZjciIC8+DQogIDxwYXRoIGQ9Ik0gMjUxLjk2NDM4MTYxNzQ5MzM2IDEyNS45OTk5OTk5OTk5OTk5OSBMIDI1MS45NjQzODE2MTc0OTMzNiAxMzMgTCAyOTguODQwNDQwNjIyMTg0OCAxMzMgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSw0Miw1KSIgaWQ9IjBkOGFmYjkyLTZlZDMtNDJjYS1hMDhjLTE5ZGUwYTNhOWI5MyIgLz4NCiAgPHBhdGggZD0iTSAxNzEuNjk0OTcxODEwMjIwNSAxNDAgTCAxNzEuNjk0OTcxODEwMjIwNSAxNTQgTCAxOTAuMTAzOTQzMDk0OTMwMyAxNTQgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSw0Miw1KSIgaWQ9IjA2OTE1ZGIyLTVhNjYtNDFhNy1hZjM3LTU4YjgzNmFhZWQyMiIgLz4NCiAgPHBhdGggZD0iTSAxOTAuMTAzOTQzMDk0OTMwMyAxNTQgTCAxOTAuMTAzOTQzMDk0OTMwMyAxNDcgTCAyNjQuNTcyMDUyODQzNDQ5NyAxNDcgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSw0Miw1KSIgaWQ9ImI0ZmM5N2JkLWQ2NjYtNDczYS1iNzViLWI4ZWZlZTZiNmM4NCIgLz4NCiAgPHBhdGggZD0iTSAxOTAuMTAzOTQzMDk0OTMwMyAxNTQgTCAxOTAuMTAzOTQzMDk0OTMwMyAxNjEgTCAyMTguNjU4ODI1NDEwNTk0MiAxNjEgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSw0Miw1KSIgaWQ9IjQ5NmQ2YzUzLTRhNTctNDEwMC1hMWU5LTBiZmI4MWYzNmNkYiIgLz4NCiAgPHBhdGggZD0iTSAwIDEzMS40Njg3NSBMIDAgMjE0LjM3NSBMIDM4LjM5MTkxODgyMjcwNzAzNCAyMTQuMzc1ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsNDIsNSkiIGlkPSIzYzQ3MmM2MS1kZDhkLTRiNmEtYWQwOC0wMjMxY2E1MWU0NTkiIC8+DQogIDxwYXRoIGQ9Ik0gMzguMzkxOTE4ODIyNzA3MDM0IDIxNC4zNzUgTCAzOC4zOTE5MTg4MjI3MDcwMzQgMTgyIEwgNTMuNTY4NDM2NDgzMTQ1NTA0IDE4MiAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iNjI4YWE2ZDMtODliMS00ZDUzLTlkM2QtOWY2ZTQwZjRmNTk1IiAvPg0KICA8cGF0aCBkPSJNIDUzLjU2ODQzNjQ4MzE0NTUwNCAxODIgTCA1My41Njg0MzY0ODMxNDU1MDQgMTc1IEwgNTYuMzczNjI5ODc3NzE2MzUgMTc1ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsNDIsNSkiIGlkPSJkY2QxMTM2MS05YzFhLTQ4YjctOTVkMi1lYjY3ZGFmYjUyNjIiIC8+DQogIDxwYXRoIGQ9Ik0gNTMuNTY4NDM2NDgzMTQ1NTA0IDE4MiBMIDUzLjU2ODQzNjQ4MzE0NTUwNCAxODkgTCA1OC41MDA3NTYxOTcyMTYzIDE4OSAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iM2Y1YjFiMzQtMTMyYS00Y2IzLThmNjUtMWJmOTJiYTM0ZTRlIiAvPg0KICA8cGF0aCBkPSJNIDM4LjM5MTkxODgyMjcwNzAzNCAyMTQuMzc1IEwgMzguMzkxOTE4ODIyNzA3MDM0IDI0Ni43NTAwMDAwMDAwMDAwMyBMIDc4LjQwMzU2NjUyNTgxOTY4IDI0Ni43NTAwMDAwMDAwMDAwMyAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iNTBlYzk3NDMtMGFhYy00MjZjLWE0OTQtY2Q2Y2ZlNjNkMTFhIiAvPg0KICA8cGF0aCBkPSJNIDc4LjQwMzU2NjUyNTgxOTY4IDI0Ni43NTAwMDAwMDAwMDAwMyBMIDc4LjQwMzU2NjUyNTgxOTY4IDIyMC41MDAwMDAwMDAwMDAwMyBMIDEzOC44MTMwNjcxOTQxODMyNSAyMjAuNTAwMDAwMDAwMDAwMDMgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSw0Miw1KSIgaWQ9Ijk4ZTZmODYxLWM5YWItNDg2Ny04MGEwLTM5YzJmNTJiZjA2MyIgLz4NCiAgPHBhdGggZD0iTSAxMzguODEzMDY3MTk0MTgzMjUgMjIwLjUwMDAwMDAwMDAwMDAzIEwgMTM4LjgxMzA2NzE5NDE4MzI1IDIwMyBMIDE4My41Njg4OTgzNjQ4MTc1IDIwMyAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iNmYwM2Y2MjEtZmJhOS00NzJiLWE3OGItZWRjYmM0MDczNGY4IiAvPg0KICA8cGF0aCBkPSJNIDEzOC44MTMwNjcxOTQxODMyNSAyMjAuNTAwMDAwMDAwMDAwMDMgTCAxMzguODEzMDY3MTk0MTgzMjUgMjM4LjAwMDAwMDAwMDAwMDAzIEwgMjA5LjEwNjQ0NDQ2MTMxMzI2IDIzOC4wMDAwMDAwMDAwMDAwMyAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iYTFjN2RhMDctMzM0ZC00ZGIwLWI4OGItMmI2YmZjYzcyNjY3IiAvPg0KICA8cGF0aCBkPSJNIDIwOS4xMDY0NDQ0NjEzMTMyNiAyMzguMDAwMDAwMDAwMDAwMDMgTCAyMDkuMTA2NDQ0NDYxMzEzMjYgMjI0IEwgMjM3Ljg5NTgwMTkzNDgxMDMgMjI0ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsNDIsNSkiIGlkPSI5OWM1ZGFlYy03OTg0LTRjZTMtOTAzMC03YzhjZDRjYTAyYzkiIC8+DQogIDxwYXRoIGQ9Ik0gMjM3Ljg5NTgwMTkzNDgxMDMgMjI0IEwgMjM3Ljg5NTgwMTkzNDgxMDMgMjE3IEwgMzAwIDIxNyAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iM2ZmOTRjNDYtNDI3OC00NjBjLTk4MjctMDMyNWMzNmExMGM3IiAvPg0KICA8cGF0aCBkPSJNIDIzNy44OTU4MDE5MzQ4MTAzIDIyNCBMIDIzNy44OTU4MDE5MzQ4MTAzIDIzMSBMIDI4My4yNjgzMzg4MzQ3MTAxIDIzMSAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iMDkyYjkwYmEtZWMwMS00YjE1LTg2MTUtMDk3NGJmNTRlY2ZkIiAvPg0KICA8cGF0aCBkPSJNIDIwOS4xMDY0NDQ0NjEzMTMyNiAyMzguMDAwMDAwMDAwMDAwMDMgTCAyMDkuMTA2NDQ0NDYxMzEzMjYgMjUyIEwgMjQxLjc3ODg5OTU1NjU5OTU4IDI1MiAiIHN0cm9rZT0iIzAwMDAwMCIgc3Ryb2tlLW9wYWNpdHk9IjEiIHN0cm9rZS13aWR0aD0iMSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBmaWxsPSJub25lIiB0cmFuc2Zvcm09Im1hdHJpeCgxLDAsMCwxLDQyLDUpIiBpZD0iYTkwNjgxMjEtMjcyMS00YWU1LWIxNDMtNDIzNTE5Y2FmMzNjIiAvPg0KICA8cGF0aCBkPSJNIDI0MS43Nzg4OTk1NTY1OTk1OCAyNTIgTCAyNDEuNzc4ODk5NTU2NTk5NTggMjQ1IEwgMjk3LjIyNjU1OTk3OTU4NzggMjQ1ICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsNDIsNSkiIGlkPSIzYTI4YTE4Mi01OWYxLTQxZTYtODkyYy03MTJmMTMyNTM0ODQiIC8+DQogIDxwYXRoIGQ9Ik0gMjQxLjc3ODg5OTU1NjU5OTU4IDI1MiBMIDI0MS43Nzg4OTk1NTY1OTk1OCAyNTkgTCAyOTEuMTc2NDg3NTc4NDY3NSAyNTkgIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1vcGFjaXR5PSIxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIgZmlsbD0ibm9uZSIgdHJhbnNmb3JtPSJtYXRyaXgoMSwwLDAsMSw0Miw1KSIgaWQ9ImRhM2Y1NmY4LTNhMzgtNGJmNC1iMTNiLWI4MWNjNDk0Mzg4OCIgLz4NCiAgPHBhdGggZD0iTSA3OC40MDM1NjY1MjU4MTk2OCAyNDYuNzUwMDAwMDAwMDAwMDMgTCA3OC40MDM1NjY1MjU4MTk2OCAyNzMgTCAxNDguOTk3MDMzNTg3NjY4OTcgMjczICIgc3Ryb2tlPSIjMDAwMDAwIiBzdHJva2Utb3BhY2l0eT0iMSIgc3Ryb2tlLXdpZHRoPSIxIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiIGZpbGw9Im5vbmUiIHRyYW5zZm9ybT0ibWF0cml4KDEsMCwwLDEsNDIsNSkiIGlkPSI3M2U2NmE5Zi05M2MwLTQ4OWMtODNhZi0yNTEzNTYzNDU5YjMiIC8+DQo8L3N2Zz4=" />
    /// </p>
    /// 
    /// ]]>
    /// </description>

    public static class MyModule
    {
        public const string Name = "Rectangular";
        public const string HelpText = "Computes the coordinates for a rectangular tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.2");
        public const string Id = "68e25ec6-5911-4741-8547-317597e1b792";
        public const ModuleTypes ModuleType = ModuleTypes.Coordinate;

        public static List<(string, string)> GetGlobalSettings()
        {
            return new List<(string, string)>()
            {
                /// <param name="Maximum default aspect ratio">
                /// This parameter determines the maximum aspect ratio that the module will use when computing
                /// the default width and height of the plot.
                /// </param>
                ( "Maximum default aspect ratio:", "NumericUpDown:1.333333333[\"1\",\"Infinity\",\"0.1\"]" )
            };
        }

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            double defaultHeight = tree.GetLeaves().Count * 14;

            double totalLength = tree.LongestDownstreamLength();
            double defaultWidth = 20 * totalLength / (from el in tree.GetChildrenRecursiveLazy() where el.Length > 0 select el.Length).MinOrDefault(0);

            if (double.IsNaN(defaultWidth))
            {
                defaultWidth = defaultHeight;
            }

            double aspectRatio = defaultWidth / defaultHeight;

            double maxAspectRatio = 4.0 / 3;

            if (TreeViewer.GlobalSettings.Settings.AdditionalSettings.TryGetValue("Maximum default aspect ratio:", out object defaultAspectRatioValue))
            {
                if (defaultAspectRatioValue is double aspectRatioValue)
                {
                    maxAspectRatio = aspectRatioValue;
                }
                else if (defaultAspectRatioValue is System.Text.Json.JsonElement element)
                {
                    maxAspectRatio = element.GetDouble();
                }
            }

            if (aspectRatio > maxAspectRatio)
            {
                defaultWidth = defaultHeight * maxAspectRatio;
            }
            else if (aspectRatio < 1 / maxAspectRatio)
            {
                defaultWidth = defaultHeight / maxAspectRatio;
            }

            return new List<(string, string)>()
            {
                ( "Tree size", "Group:2" ),
                
                /// <param name="Width:" default="20 $\cdot t$ / $\min \ l$">
                /// This parameter determines the width of the area covered by the tree.
                /// 
                /// $t$ is the total length from the root node of the tree to the farthest tip; $\min \ l$
                /// is the minimum branch length that is $&gt; 0$. If the default width cannot be computed
                /// (e.g. because the tree does not have any branch length information), the default width
                /// is equal to the default height.
                /// 
                /// The default width and height are adjusted to keep an aspect ratio below the [Maximum default aspect ratio](#maximum-default-aspect-ratio).
                /// </param>
                ( "Width:", "NumericUpDown:" + defaultWidth.ToString(0) + "[\"0\",\"Infinity\"]" ),
                
                /// <param name="Height:" default="14 $\cdot n$">
                /// This parameter determines the height of the area covered by the tree.
                /// 
                /// The default width and height are adjusted to keep an aspect ratio between 9:16 and 16:9.
                /// </param>
                ( "Height:", "NumericUpDown:" + defaultHeight.ToString(0) + "[\"0\",\"Infinity\"]" ),

                ( "Rotation", "Group:2" ),
                
                /// <param name="Rotation:">
                /// This parameter determines the rotation of the tree coordinates.
                /// </param>
                ( "Rotation:", "Slider:0[\"0\",\"360\",\"0°\"]" ),
                
                /// <param name="FixedRotations" display="Fixed rotations">
                /// These buttons can be used to quickly set the value of the [Rotation](#rotation) to
                /// predefined values.
                /// </param>
                ( "FixedRotations", "Buttons:[\"0°\",\"90°\",\"180°\",\"270°\"]" ),
                
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

            };

            parametersToChange = new Dictionary<string, object>() { { "Apply", false }, { "FixedRotations", -1 } };

            int fixRot = (int)currentParameterValues["FixedRotations"];

            if (fixRot >= 0)
            {
                parametersToChange.Add("Rotation:", (double)(fixRot * 90));
                return true;
            }

            return (bool)currentParameterValues["Apply"];
        }

        public static Dictionary<string, Point> GetCoordinates(TreeNode tree, Dictionary<string, object> parameterValues)
        {
            List<TreeNode> nodes = tree.GetChildrenRecursive();

            Dictionary<string, Point> storedPos = new Dictionary<string, Point>();

            Dictionary<string, double> upstreamLengths = new Dictionary<string, double>();

            bool allNaN = true;

            foreach (TreeNode node in nodes)
            {
                if (node.Parent == null)
                {
                    upstreamLengths[node.Id] = 0;
                }
                else
                {
                    if (!double.IsNaN(node.Length))
                    {
                        upstreamLengths[node.Id] = upstreamLengths[node.Parent.Id] + node.Length;
                        allNaN = false;
                    }
                    else
                    {
                        upstreamLengths[node.Id] = upstreamLengths[node.Parent.Id];
                    }
                }
            }

            if (allNaN)
            {
                foreach (TreeNode node in nodes)
                {
                    if (node.Parent == null)
                    {
                        upstreamLengths[node.Id] = 0;
                    }
                    else
                    {
                        upstreamLengths[node.Id] = upstreamLengths[node.Parent.Id] + 1;
                    }
                }
            }

            double totalLength = upstreamLengths.Values.Max();

            if (totalLength == 0)
            {
                totalLength = 1;
            }

            Dictionary<string, double> yMultipliersCache = new Dictionary<string, double>();

            Dictionary<string, int> leafIndices = new Dictionary<string, int>();

            foreach (TreeNode leaf in tree.GetLeaves())
            {
                leafIndices[leaf.Id] = leafIndices.Count;
            }


            double getYMultiplier(TreeNode tree)
            {
                if (yMultipliersCache.TryGetValue(tree.Id, out double value))
                {
                    return value;
                }
                else
                {
                    if (tree.Children.Count == 0)
                    {
                        int ind = leafIndices[tree.Id];

                        double val = (ind + 0.5) / leafIndices.Count;
                        yMultipliersCache[tree.Id] = val;
                        return val;
                    }
                    else if (!tree.Attributes.ContainsKey("0c3400fd-8872-4395-83bc-a5dc5f4967fe") || (tree.Parent != null && tree.Parent.Attributes.ContainsKey("0c3400fd-8872-4395-83bc-a5dc5f4967fe")))
                    {
                        double val = 0.5 * (getYMultiplier(tree.Children[0]) + getYMultiplier(tree.Children[^1]));
                        yMultipliersCache[tree.Id] = val;
                        return val;
                    }
                    else
                    {
                        List<TreeNode> leaves = tree.GetLeaves();
                        double val = 0.5 * (getYMultiplier(leaves[0]) + getYMultiplier(leaves[^1]));
                        yMultipliersCache[tree.Id] = val;
                        return val;
                    }
                }
            }

            double width = (double)parameterValues["Width:"];
            double height = (double)parameterValues["Height:"];

            double rotation = (double)parameterValues["Rotation:"] * Math.PI / 180;

            for (int i = 0; i < nodes.Count; i++)
            {
                double x = upstreamLengths[nodes[i].Id] / totalLength * width;
                double y = getYMultiplier(nodes[i]) * height;

                if (rotation > 0)
                {
                    double newX = x * Math.Cos(rotation) - y * Math.Sin(rotation);
                    double newY = x * Math.Sin(rotation) + y * Math.Cos(rotation);
                    x = newX;
                    y = newY;
                }

                storedPos[nodes[i].Id] = new Point(x, y);
            }

            double rootLength = tree.Length >= 0 ? tree.Length : 0.1 * totalLength;
            double rootX = -rootLength / totalLength * width;
            double rootY = getYMultiplier(nodes[0]) * height;

            storedPos[Modules.RootNodeId] = new Point(rootX * Math.Cos(rotation) - rootY * Math.Sin(rotation), rootX * Math.Sin(rotation) + rootY * Math.Cos(rotation));

            storedPos[Id] = new Point(width / totalLength * Math.Cos(rotation), width / totalLength * Math.Sin(rotation));

            return storedPos;
        }
    }
}
