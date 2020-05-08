# HGGMoodleClient

A client written c# that can access the (unoffical) <a href="https://moodle.org/">moodle</a> api (even if the administrator has disabled the offical api) using "html-parsing" and json-rpc.

## Moodle Endpoint Documentation
### All Functions
https://docs.moodle.org/dev/Web_service_API_functions <br/>
<b>Important!</b> Is function "<i>Available</i> [for] <i>AJAX</i>"?
### Paramater Documentation
https://github.com/moodle/moodle <br/>
Search for "<i>externallib.php</i>" file<i>s</i>. There you can find the paramteter declaration.<br/>
You'll find this file multiple times; choos the one with the correct enpoint type (look at the path the function is in).

## "API" Documentation

<ol>
  <li>
    <b>Get Content of <code>https://{$Host}/moodle/blocks/exa2fa/login/</code></b><br/>
    <b>Extract LoginToken:</b><br/>
    XPAth: <code>//input[@name='logintoken']</code> bzw <code>GetElementByName("logintoken")</code><br/>
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
    
```json
[{"index":0, "methodname":"core_course_get_enrolled_courses_by_timeline_classification", "args" :{"offset":0,"limit":100,"classification":"all","sort":"fullname"}}]
```
    
  </li>
  <li>
    <b>Well done!</b><br/>
    All functions that can be invoked using this method are listed in the link above. Just copy the function name paste it in the json    above (same endpoint) and look up the according parameter (described above).<br/>
    This method works with every function that may be used with ajax (specification in link).<br/>
    It even works if the administrator has disabled the offical api!<br/>
    Good luck!
  </li>
</ol>
<h2>Interesting functions with parameters</h2>
<ul>
  <li>
    <code>core_course_get_enrolled_courses_by_timeline_classification</code><br/>
    <i>List all courses (for person)</i><br/>
    Parameters:<br/>
    <table>
      <tr>
        <th>Name</th>
        <th>Recommended Value</th>
        <th>Description</th>
      </tr>
      <tr>
        <td>offset</td>
        <td>0</td>
        <td>First index in result</td>
      </tr>
      <tr>
        <td>limit</td>
        <td>100</td>
        <td>Max count of courses returned</td>
      </tr>
      <tr>
        <td>classification</td>
        <td>all</td>
        <td>???</td>
      </tr>
      <tr>
        <td>sort</td>
        <td>fullname</td>
        <td>How to sort result</td>
      </tr>
    </table>
    Example body:<br/>
      
```json
[{"index":0, "methodname":"core_course_get_enrolled_courses_by_timeline_classification", "args":{"offset":0, "limit":100, "classification":"all", "sort":"fullname"}}]
```
    
  </li>
  <li>
    <code>core_course_get_updates_since</code><br/>
    <i>Return changes that were made to a course since a specific timestamp</i><br/>
    Parameters:<br/>
    <table>
      <tr>
        <th>Name</th>
        <th>Recommended Value</th>
        <th>Description</th>
      </tr>
      <tr>
        <td>courseid</td>
        <td></td>
        <td>Id of course (get with function above)</td>
      </tr>
      <tr>
        <td>since</td>
        <td></td>
        <td>Timestamp</td>
      </tr>
    </table>
    Example body:<br/>
      
```json
[{"index":0, "methodname":"core_course_get_updates_since", "args":{"courseid": 12345, "since": 1588929434}}]
```
    
  </li>
  <li>
    <code>core_message_send_instant_messages</code><br/>
    <i>Sends message to user (by id):</i><br/>
    Parameters:<br/>
    <table>
      <tr>
        <th>Name</th>
        <th>Recommended Value</th>
        <th>Description</th>
      </tr>
      <tr>
        <td>messages</td>
        <td></td>
        <td>Array of beneath structure</td>
      </tr>
    </table>
    <code>messages</code> structure:<br/>
    <table>
      <tr>
        <th>Name</th>
        <th>Type</th>
        <th>Description</th>
      </tr>
      <tr>
        <td>touserid</td>
        <td>integer</td>
        <td>Receiving user id</td>
      </tr>
      <tr>
        <td>text</td>
        <td>string</td>
        <td>Message</td>
      </tr>
    </table>
    Example body:<br/>
      
```json
[{"index":0, "methodname":"core_message_send_instant_messages", "args":{"messages":[{"touserid": 12345, "text": "Bye World!" }]}}]
```
    
  </li>
</ul>
