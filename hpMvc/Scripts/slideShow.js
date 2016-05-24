var element = document.getElementById("Slideshow");
var duration = 2000; /* fade duration in millisecond */
var hidtime = 100; /* time to stay hidden */
var showtime = 6000; /* time to stay visible */

var running = 0; /* Used to check if fade is running */
var iEcount = 1; /* Element Counter */

var iTotalE = element.getElementsByTagName('div').length;


function SetOpa(Opa) {
  element.style.opacity = Opa;
  element.style.MozOpacity = Opa;
  element.style.KhtmlOpacity = Opa;
  element.style.filter = 'alpha(opacity=' + (Opa * 100) + ');';
}
function StartFade() {
  if (running != 1) {
   running = 1;
   setTimeout("fadeOut()", (showtime))
  }
}
function fadeOut() {
	  for (i = 0; i <= 1; i += 0.01) {
		setTimeout("SetOpa(" + (1 - i) +")", i * duration);
	  }
		setTimeout("FadeIn()", (duration + hidtime));
}
function FadeIn() {
  for (i = 0; i <= 1; i += 0.01) {
    setTimeout("SetOpa(" + i +")", i * duration);
  }
   if (iEcount == iTotalE) {
    iEcount = 1;
    document.getElementById("slice" + iEcount).style.display = "block";
    document.getElementById("slice" + iTotalE).style.display = "none";
   } else {
    document.getElementById("slice" + (iEcount + 1)).style.display = "block";
    document.getElementById("slice" + iEcount).style.display = "none";
    iEcount = iEcount+1;
   }
   setTimeout("fadeOut()", (duration + showtime));
}