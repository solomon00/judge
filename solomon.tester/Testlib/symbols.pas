{ Copyright(c) SPb-IFMO CTD Developers, 2000 }
{ Copyright(c) Anton Sukhanov, 1996 }

{ $Id$ }

{ String processing fuctions }

{
  Modifications log: 
    dd.mm.yyyy  modified by             modification
    01.09.2000  Andrew Stankevich       all comments translated to 
                                          english
    01.09.2000  Andrew Stankevich       cmpasc fixed, now no asm code,
                                          so unit is portable
}

{$A-,B-,D+,E-,F+,G+,I+,L+,N+,O-,P-,Q-,R-,S-,T-,V+,X+,Y+}
{$M 16384,0,655360}

unit Symbols;

INTERFACE

function StrError: boolean;

function GetParam (s: string; n: integer): string;
         { Takes N-th parameter respecting quotes }
function CutParam (var s: string; n: integer): string;
         { Takes N-th parameter respecting quotes.
           Removes result from source. }

function cmpbin (var a, b; len: integer): integer;
         { Binary comparision, case sensitive }
function cmpasc (var a, b; len: integer): integer;
         { ASCII comparision, case insensitive }
function cmpstr (s1, s2: string): integer;
         { String comparision, case insensitive }

function CalcWords (s: string): integer;
         { Calculates number of words in s }

function E_str (x: longint; width: integer; base: integer; fill: char): string;
         { Convert integer to string, length no less then width,
           using base, filling rightwards space with fill char }
function str (x: longint; width: integer): string;
         { Convert integer to string in base 10, use width characters }
function RealStr (x: real; w, d: integer): string;
         { Convert real to string, use w chars, d after point }

function compress (s: string): string;
         { Delete leading and trailing spaces from string, return it }
procedure FastCompress (var s: string);
         { Delete leading and trailing spaces from string, modify it }

function Bin2Str (var a; len: integer): string;
         { Convert array to string }

function  UpStr (s: string): string;
         { Convert string to upper case. Supports russian. }
function UpChar (c: char): char;
         { Convert char to upper case. Supports russian. }
function LowChar (c: char): char;
         { Convert string to lower case. Supports russian. }
procedure FastUpcaseStr (var s: string);
         { Convert string to upper case. Supports russian. Modifies source. }
function LoStr (s: string): string;
         { Convert string to lower case. Supports russian. }

function IntegerVal (s: string): integer;
         { Convert string to integer }
function LongintVal (s: string): longint;
         { Convert string to longint }
function RealVal (s: string): real;
         { Convert string to real }

function split (s: string; c: char; var a, b: string): boolean;
         { Split string s to a and b using c as separator }

function RepeatStr (s: string; cnt: integer): string;

function AliLeft (s: string; width: integer): string;
         { Align s to left side of string of given length }
function AliCentre (s: string; width: integer): string;
function AliCenter (s: string; width: integer): string;
         { Align s to center of string of given length }
function AliRight (s: string; width: integer): string;
         { Align s to right side of string of given length }
function AliLR (left: string; right: string; width: integer): string;
         { Align left to left side and right to right side
           of string of given length }

function DeleteQuoters (s: string): string;
         { Delete all quotes }

IMPLEMENTATION

var error: boolean;

function StrError: boolean;
begin
   StrError := error
end;

function DeleteQuoters (s: string): string; 
var p: integer;
begin
   while pos (s, '''') <> 0 do delete (s, pos (s,''''),1);
   while pos (s, '"') <> 0 do delete (s, pos (s,'"'),1);
   DeleteQuoters := s;
end;

Function LocateParam (s: string; n: integer; var i, j: integer): Boolean;
var len: integer;
begin
   LocateParam := False;
   if n < 1 then Exit;

   j := 0; len := length (s);
   repeat
      i := j+1;
      while (i<=len) and (s [i] = ' ') do inc (i);
      if i > len then exit;
      j := i+1;

      if (s [i] = '''') or (s [i] = '"') then
         while (j<=len) and (s [j] <> s [i]) do inc (j)
      else
         while (j<=len) and (s [j] <> ' ') do inc (j);

      if (j > len) or (s [j] = ' ') then dec (j);
      dec (n);
   until (n = 0);
   LocateParam := True;
end;

function GetParam (s: string; n: integer): string;
var i, j: integer;
    help: string;
begin
   if not LocateParam (s, n, i, j) then GetParam := ''
   else begin
      if (s [i] = '''') or ( s [i] = '"') then inc (i);
      if (s [j] = '''') or ( s [j] = '"') then dec (j);
      if i <= j then  GetParam := Copy (s, i, j-i+1)
                else  GetParam := ' ';
   end;
end;

function CutParam (var s: string; n: integer): string;
var i, j, i1, j1: integer;
    help: string;
begin
   if not LocateParam (s, n, i, j) then CutParam := ''
   else begin
      i1 := i; j1 := j;
      if (s [i] = '''') or ( s [i] = '"') then inc (i);
      if (s [j] = '''') or ( s [j] = '"') then dec (j);
      if i <= j then  CutParam := Copy (s, i, j-i+1)
                else  CutParam := ' ';
      delete (s, i1, j1-i1+1);
   end;
end;



function cmpbin (var a, b; len: integer): integer;
var x: array [1..65520] of char absolute a;
    y: array [1..65520] of char absolute b;
    i: word;
begin
   for i := 1 to len do
      if x [i] <> y [i] then
      begin
         if x [i] < y [i] then cmpbin := -1
                          else cmpbin := +1;
         exit
      end;

   cmpbin := 0;
end;

function cmpasc (var a, b; len: integer): integer;
label c1, c2;
var
  _a: array [1..maxint] of char absolute a;
  _b: array [1..maxint] of char absolute b;
  i: longint;
begin
  cmpasc := 0;
  for i := 1 to len do
  begin
    if upchar(_a[i]) > upchar(_b[i]) then 
    begin
      cmpasc := 1;
      exit;
    end;
    if upchar(_a[i]) < upchar(_b[i]) then
    begin
      cmpasc := -1;
      exit;
    end;
  end;
end;

function cmpstr (s1, s2: string): integer;
begin
   if length (s1) < length (s2) then
      case cmpasc (s1 [1], s2 [1], length (s1)) of
          0: cmpstr := -1;
          1: cmpstr := 1;
         -1: cmpstr := -1
      end
   else if length (s1) > length (s2) then
      case cmpasc (s1 [1], s2 [1], length (s2)) of
          0: cmpstr := 1;
          1: cmpstr := 1;
         -1: cmpstr := -1
      end
   else cmpstr := cmpasc (s1 [1], s2 [1], length (s1));
end;

function UpStr (s: string): string;
var i: integer; c: char;
begin
   for i := 1 to length (s) do
   begin
      c := s [i];
        if (c >= 'a') and (c <= 'z') then s [i] := chr (ord(c)-ord('a')+ord('A'))
      else
        if (c >= '†') and (c <= 'Ø') then s [i] := chr (ord(c)-ord('†')+ord('Ä'))
      else
        if (c >= '‡') and (c <= 'Ô') then s [i] := chr (ord(c)-ord('‡')+ord('ê'));
   end;
   UpStr := s
end;

function UpChar (c: char): char;
begin
      if (c >= 'a') and (c <= 'z') then UpChar := chr (ord(c)-ord('a')+ord('A'))
   else
      if (c >= '†') and (c <= 'Ø') then UpChar := chr (ord(c)-ord('†')+ord('Ä'))
   else
     if (c >= '‡') and (c <= 'Ô') then UpChar := chr (ord(c)-ord('‡')+ord('ê'))
   else
     UpChar := c; 
end;

function LowChar (c: char): char;
begin
      if (c >= 'A') and (c <= 'Z') then LowChar := chr (ord(c)-ord('A')+ord('a'))
   else
      if (c >= 'Ä') and (c <= 'è') then LowChar := chr (ord(c)-ord('Ä')+ord('†'))
   else
     if (c >= 'ê') and (c <= 'ü') then LowChar := chr (ord(c)-ord('ê')+ord('‡'))
   else
     LowChar := c; 
end;


procedure FastUpcaseStr (var s: string);
var i: integer; c: char;
begin
   for i := 1 to length (s) do
   begin
      c := s [i];
        if (c >= 'a') and (c <= 'z') then s [i] := chr (ord(c)-ord('a')+ord('A'))
      else
        if (c >= '†') and (c <= 'Ø') then s [i] := chr (ord(c)-ord('†')+ord('Ä'))
      else
        if (c >= '‡') and (c <= 'Ô') then s [i] := chr (ord(c)-ord('‡')+ord('ê'));
   end;
end;

function LoStr (s: string): string;
var i: integer; c: char;
begin
   for i := 1 to length (s) do
   begin
      c := s [i];
        if (c >= 'A') and (c <= 'Z') then s [i] := chr (ord(c)-ord('A')+ord('a'))
      else
        if (c >= 'Ä') and (c <= 'è') then s [i] := chr (ord(c)-ord('Ä')+ord('†'))
      else
        if (c >= 'ê') and (c <= 'ü') then s [i] := chr (ord(c)-ord('ê')+ord('‡'));
   end;
   LoStr := s
end;

function LongintVal (s: string): longint;
const digits: set of char = ['1','2','3','4','5','6','7','8','9','0'];
var sgn: integer;
    i: integer;
    x: longint;
begin
   error := false;
   s [length(s)+1] := #0;
   i := 1; while s [i] = ' ' do i := i+1;
   if s [i] = '-' then begin sgn := -1; inc (i) end else sgn := 1;
   if not (s [i] in digits) then begin error := true; exit end;
   x := 0;
   repeat
      x := x * 10 + ord (s [i]) - ord ('0'); inc (i);
   until not (s [i] in Digits);
   x := x * sgn;
   while s [i] = ' ' do inc (i);
   error := s [i] <> #0;
   LongintVal := x;
end;

function IntegerVal (s: string): integer;
const digits: set of char = ['1','2','3','4','5','6','7','8','9','0'];
var sgn: integer;
    i: integer;
    x: longint;
begin
   error := false;
   s [length(s)+1] := #0;
   i := 1; while s [i] = ' ' do i := i+1;
   if s [i] = '-' then begin sgn := -1; inc (i) end else sgn := 1;
   if not (s [i] in digits) then begin error := true; exit end;
   x := 0;
   repeat
      x := x * 10 + ord (s [i]) - ord ('0'); inc (i);
   until not (s [i] in Digits);
   x := x * sgn;
   while s [i] = ' ' do inc (i);
   error := s [i] <> #0;
   IntegerVal := x;
end;

function RealVal (s: string): real;
var r: real;
    Code: integer;
begin
   val (s, r, Code);
   RealVal := r;
   error := Code <> 0;
end;

function RealStr (x: real; w, d: integer): string;
var r: string;
begin
   system.Str (x:w:d, r);
   RealStr := r
end;

function RepeatStr (s: string; cnt: integer): string;
var res: string;
    i: integer;
begin
   res := ''; for i := 1 to cnt do res := res + s;
   repeatstr := res
end;

function AliLeft (s: string; width: integer): string;
begin
   if length (s) > width then AliLeft := copy (s, 1, width)
                         else AliLeft := s + repeatstr (' ', width-length(s));
end;

function AliLR (left: string; right: string; width: integer): string;
var x: integer;
begin
   x := Length (left) + length (right);
   if x > width then
      AliLR := copy (left + right, 1, width)
   else
      AliLR := left + repeatstr (' ', width - x) + right
end;

function AliCentre (s: string; width: integer): string;
begin
   if length (s) > width then AliCentre := copy (s, 1 + (length (s)-width) div 2, width)
                         else AliCentre := repeatstr (' ', (-length (s)+width) div 2) +
                                           s +
                                           repeatstr (' ',width - length (s) - (-length (s)+width) div 2);
end;

function AliCenter (s: string; width: integer): string;
begin
  alicenter := alicentre(s, width);
end;

function AliRight (s: string; width: integer): string;
begin
   if length (s) > width then AliRight := copy (s, 1, width)
                         else AliRight := repeatstr (' ', width-length(s)) + s;
end;

function Bin2Str (var a; len: integer): string;
var r: string;
    s: array [1..65520] of char absolute a;
begin
   r [0] := chr (len); move (a, r [1], len);
   Bin2Str := r
end;

function CalcWords (s: string): integer; 
var i: integer;
    r: integer;
begin
   if s = '' then calcwords := 0
   else begin
       r := 0;
       for i := 1 to length (s)-1 do
          if (s [i] = ' ') and (s [i+1] <> ' ') then inc (r);
       if s [1] <> ' ' then inc (r);
       calcwords := r
   end;
end;

function E_Str (x: longint; width: integer; base: integer; fill: char): string;
const Digits: array [0..15] of char = '0123456789ABCDEF';
var r: string;
    sgn: integer;
begin
   if x < 0 then sgn := 1 else sgn := 0;
   x := abs (x);
   r := '';
   repeat
      r := Digits [x mod base] + r; x := x div base;
      dec (width);
   until (x = 0) or (width=0);
   if (sgn = 1) then r := '-' + r;
   while width - sgn > 0 do begin r := fill + r; dec (width) end;
   E_Str := r;
end;

function Str (x: longint; width: integer): string;
begin
   Str := E_Str (x, width, 10, ' ');
end;

procedure FastCompress (var s: string);
var i, j: integer;
begin
   i := 1; while (i<=length(s)) and (s [i] = ' ') do inc (i);
   if i > length (s) then s := ''
   else begin
     j := length (s); while s [j] = ' ' do dec (j);
     s := copy (s, i, j-i+1);
  end;
end;

function Compress (s: string): string;
begin
   FastCompress (s);
   Compress := s;
end;

function split (s: string; c: char; var a, b: string): boolean;
var i: integer;
begin
   i := 1;
   while (i<=length(s)) and (s [i] = ' ') do inc (i);
   while (i<=length(s)) and (s [i] <> c) do inc (i);

   if i > length (s) then
   begin
      a := compress (s);
      b := '';
      split := false;
   end
   else begin
      a := compress (copy (s, 1, i-1));
      b := compress (copy (s, i+1, length (s)));
      split := true;
   end;
end;

BEGIN 

END.