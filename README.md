# HGGMoodleClient

A client written c# that can access the (unoffical) moodle api (even if the administrator has disabled the offical api) using "html-parsing" and json-rpc.

## Moodle Endpoint Documentation
### All Functions
https://docs.moodle.org/dev/Web_service_API_functions <br/>
<b>Important!</b> Is function "<i>Available</i> [for] <i>AJAX</i>"?
### Paramater Documentation
https://github.com/moodle/moodle <br/>
Search for "<i>externallib.php</i>" file<i>s</i>. There you can find the paramteter declaration.<br/>
You'll find this function multiple times; choos the one with the correct enpoint type (look at the path the function is in).

## "API" Documentation

<ol>
  <li>
    <b>Get Content of <code>https://{$Host}/moodle/blocks/exa2fa/login/</code></b><br/>
    <b>Extract LoginToken:</b><br/>
    XPAth: <code>//input[@name='logintoken']</code><br/>
    Get <code>Value</code> attribute of found <code>HTMLInputElement</code>
  </li>
  <li>
    <b>Login</b>
    <ol>
      <li>
        <b>Send formular via <code>POST</code></b><br/>
        Headers:
        <table>
          <tr>
            <th>Name</th>
            <th>Value</th>
          </tr>
          <tr>
            <td>X-Requested-With</td>
            <td>XMLHttpRequest</td>
          </tr>
          <tr>
            <td>Accept</td>
            <td>application/json, text/javascript, */*; q=0.01</td>
          </tr>
          <tr>
            <td>ContentType</td>
            <td>application/x-www-form-urlencoded</td>
          </tr>
        </table>
        Body:<br/>
        <code>ajax=true&username={$username}&password={$password}&logintoken={$logintoken}&anchor=&token=</code>
      </li>
      <li>
        <b>Parse response (It's Json!)</b><br/>
        <ol>
          <li>
            Check for key <code>error</code>: If error=false everything is fine!<br/>        
          </li>
          <li>
            Downloadload HTML from url provided in key <code>url</code><br/>        
          </li>
          <li>
            <b>Extract SessionKey:</b><br/>
            Regex: <code>"\"sesskey\":\"(([a-z]|[0-9])*)\""</code><br/>
            Use value of second group.
          </li>
        </ol>
      </li>
    </ol>
  </li>    
  <li>
    <b>From now on it's only Json(-RPC)!</b><br/>
    https://youtu.be/FmeyLUKHI4Q<br/>
    https://en.wikipedia.org/wiki/JSON-RPC
  </li>
  <li>
    <b>Sample Request</b><br/>
    <i>List all courses in a specific order</i><br/>
    URL:<br/>
    <code>https://{$Host}/moodle/lib/ajax/service.php?sesskey={$SessionKey}</code><br/>
    Body:<br/>
    <code>[{"index":0,
      "methodname":"core_course_get_enrolled_courses_by_timeline_classification",
      "args":{"offset":0,"limit":100,"classification":"all","sort":"fullname"}
      }]</code>
  </li>
  <li>
    <b>Well done!</b><br/>
    All functions that can be invoked using this method are listed in the link above. Just copy the function name paste it in the json    above (same endpoint) and look up the according parameter (described above).<br/>
    This method works with every function that may be used with ajax (specification in link).<br/>
    It even works if the administrator has disabled the offical api!<br/>
    Good luck!
  </li>
</ol>
