## ReCollect.RichTextLabel

This library is yet another implementation of a Rich-Text UILabel that allows
for HTML to be parsed and emitted as a `NSMutableAttributedString`. This
implementation is implemented in C# for use in a Xamarin iOS application.

HTML is parsed using the [Html Agility Pack](http://htmlagilitypack.codeplex.com/), 
and then emitted as rich-text that can be set on the ReCollect.RichTextLabel.

### Supported HTML

The following HTML is supported:
* `<p></p>`, `<div></div>`: Become strings with a trailing newline
* `<i></i>`, `<em></em>`: Italics
* `<b></b>`, `<strong></strong>`: Bold
* `<a href="URL"></a>`: Clickable link
* `<font></font>`
  * Attribute `color` with supported format: `#FFFFFF`

