<?php
 header("Content-Type: application/json");
 $input = json_decode(file_get_contents("php://input"), true);
 $name = $input["name"] ?? "";
 $score = $input["score"] ?? null;
 if ($name === "" || $score === null) {
 echo json_encode(["ok" => false, "error" => "Missing name or score"]);
 exit; 
 }
 try {
 $pdo = new PDO("mysql:host=localhost;dbname=db_highscoresbullethell;charset=utf8", "root", "");
 $stmt = $pdo->prepare("INSERT INTO highscores (name, score) VALUES (?, ?)");
 $ok = $stmt->execute([$name, $score]);
 echo json_encode(["ok" => $ok]);
 } catch (Exception $e) {
 echo json_encode(["ok" => false, "error" => $e->getMessage()]); 
 }